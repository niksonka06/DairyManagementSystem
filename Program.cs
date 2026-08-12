using DairyManagementSystem.Data;
using DairyManagementSystem.Data.Seed;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Repositories;
using DairyManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ISocietyService, SocietyService>();
builder.Services.AddScoped<IFarmerService, FarmerService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

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
app.UseStaticFiles();

app.UseRouting();

// Must sit between UseRouting and endpoint mapping: routing decides *which*
// endpoint matched, then these decide *who's allowed* to hit it.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
