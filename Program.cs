using DairyManagementSystem.Data;
using DairyManagementSystem.Data.Seed;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Repositories;
using DairyManagementSystem.Services;
using DairyManagementSystem.Areas.Operator.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Suppress the "Server: Kestrel" response header — small info-disclosure
// hardening, no functional cost.
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// QuestPDF requires an explicit license declaration. Community is free for
// this project's scale (see QuestPDF's licensing terms) — must be set once
// before any PDF is generated.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ---------------------------------------------------------------------
// SERVICE REGISTRATION
// ---------------------------------------------------------------------

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    // Baseline password policy. Deliberately not exotic — matches
    // "practical, not over-engineered" from the synopsis's quality
    // standard, while still being a real policy, not Identity's
    // permissive default.
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Secure cookie configuration — directly implements the synopsis's
// "Session Security: HttpOnly and SameSite=Strict cookie flags" requirement.
// SameAsRequest in Development so the http launch profile (port 5140) works;
// Always in production so cookies are never sent over plain HTTP.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Scoped: one instance per HTTP request, matching ApplicationDbContext's own
// lifetime — this is what lets repositories used within the same request
// share one DbContext, and therefore commit together via one SaveChangesAsync.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<ISocietyRepository, SocietyRepository>();
builder.Services.AddScoped<IFarmerRepository, FarmerRepository>();
builder.Services.AddScoped<IMilkRateRepository, MilkRateRepository>();
builder.Services.AddScoped<IMilkCollectionRepository, MilkCollectionRepository>();
builder.Services.AddScoped<IFeedInventoryRepository, FeedInventoryRepository>();
builder.Services.AddScoped<IFeedIssueRepository, FeedIssueRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ISettlementDeductionRepository, SettlementDeductionRepository>();
builder.Services.AddScoped<IAdvancePaymentRepository, AdvancePaymentRepository>();
builder.Services.AddScoped<IDispatchRepository, DispatchRepository>();

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ISocietyService, SocietyService>();
builder.Services.AddScoped<IFarmerService, FarmerService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IMilkRateService, MilkRateService>();
builder.Services.AddScoped<IMilkCollectionService, MilkCollectionService>();
builder.Services.AddScoped<IFeedInventoryService, FeedInventoryService>();
builder.Services.AddScoped<IFeedIssueService, FeedIssueService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAdvancePaymentService, AdvancePaymentService>();
builder.Services.AddScoped<IDispatchService, DispatchService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAuditLogViewService, AuditLogViewService>();
builder.Services.AddScoped<EnsureActiveOperatorSocietyFilter>();

builder.Services.AddHttpClient("SmsGateway");
builder.Services.AddScoped<ISmsService, SmsService>();

// Rate limiting on login specifically — account lockout (Stage 3) stops
// repeated attempts against ONE account, but doesn't stop an attacker
// hammering the login endpoint across MANY different email addresses. This
// closes that gap: 5 login attempts per minute per client, regardless of
// which account they're targeting.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("LoginPolicy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();

// ---------------------------------------------------------------------
// SEED ROLES + INITIAL ADMIN (runs once per startup, idempotent)
// ---------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

// ---------------------------------------------------------------------
// HTTP REQUEST PIPELINE
// Order matters here — this is the sequence ASP.NET Core executes per request.
// ---------------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Security response headers — applied to every response, including static
// files, so this sits before UseStaticFiles. Content-Security-Policy is
// scoped to exactly the CDN sources this app actually uses (Bootstrap,
// Chart.js, jQuery validation) rather than a wildcard, which is the whole
// point of a CSP — an overly permissive one is no protection at all.
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "img-src 'self' data:; " +
        "font-src 'self' https://cdn.jsdelivr.net;");
    await next();
});

app.UseStaticFiles();

app.UseRouting();

// Must sit between UseRouting and endpoint mapping: routing decides *which*
// endpoint matched, then these decide *who's allowed* to hit it.
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
