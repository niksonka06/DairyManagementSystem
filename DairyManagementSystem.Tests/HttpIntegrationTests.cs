using System.Net;
using System.Text.RegularExpressions;
using DairyManagementSystem.Data;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DairyManagementSystem.Tests
{
    public sealed class DairyWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public const string Password = "TestPass1";
        public const string AdminEmail = "admin@dairysystem.local";
        public const string OperatorEmail = "operator@test.local";
        public const string FarmerEmail = "farmer1@test.local";
        public const string Farmer2Email = "farmer2@test.local";

        private readonly string _dbName = Guid.NewGuid().ToString();

        public int SocietyId { get; private set; }
        public int Farmer1Id { get; private set; }
        public int Farmer2Id { get; private set; }
        public int CollectionFarmer1Id { get; private set; }
        public int CollectionFarmer2Id { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                foreach (var descriptor in services.Where(IsDbContextRegistration).ToList())
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));
            });
        }

        private static bool IsDbContextRegistration(ServiceDescriptor descriptor)
        {
            if (descriptor.ServiceType == typeof(ApplicationDbContext)
                || descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
            {
                return true;
            }

            return descriptor.ServiceType.IsGenericType
                   && descriptor.ServiceType.GetGenericArguments().Any(t => t == typeof(ApplicationDbContext));
        }

        public async Task InitializeAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var admin = await users.FindByEmailAsync(AdminEmail);
            if (admin is not null)
            {
                admin.MustChangePassword = false;
                await users.UpdateAsync(admin);
            }

            var society = new Society
            {
                SocietyName = "Test Society",
                RegistrationNo = "REG-TEST-1",
                Address = "Test",
                ContactPhone = "9999999999",
                IsActive = true,
                RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
            };
            db.Societies.Add(society);
            await db.SaveChangesAsync();
            SocietyId = society.SocietyID;

            var operatorUser = await CreateUserAsync(users, OperatorEmail, "Test Operator", Roles.Operator, SocietyId);
            var farmerUser1 = await CreateUserAsync(users, FarmerEmail, "Farmer One", Roles.Farmer, SocietyId);
            var farmerUser2 = await CreateUserAsync(users, Farmer2Email, "Farmer Two", Roles.Farmer, SocietyId);

            var farmer1 = new Farmer
            {
                SocietyID = SocietyId,
                UserID = farmerUser1.Id,
                FarmerCode = "F001",
                FullName = "Farmer One",
                Phone = "9000000001",
                Address = "A",
                BankAccountNo = "111",
                BankName = "Bank",
                IFSC = "SBIN0000001",
                IsActive = true,
                RowVersion = new byte[] { 1 }
            };
            var farmer2 = new Farmer
            {
                SocietyID = SocietyId,
                UserID = farmerUser2.Id,
                FarmerCode = "F002",
                FullName = "Farmer Two",
                Phone = "9000000002",
                Address = "B",
                BankAccountNo = "222",
                BankName = "Bank",
                IFSC = "SBIN0000001",
                IsActive = true,
                RowVersion = new byte[] { 1 }
            };
            db.Farmers.AddRange(farmer1, farmer2);
            await db.SaveChangesAsync();
            Farmer1Id = farmer1.FarmerID;
            Farmer2Id = farmer2.FarmerID;

            db.MilkRates.Add(new MilkRate
            {
                SocietyID = SocietyId,
                FatPercentFrom = 2.5m,
                FatPercentTo = 9,
                SnfPercentFrom = 7.5m,
                SnfPercentTo = 11,
                ClrFrom = 0,
                ClrTo = 50,
                RatePerLitre = 40,
                EffectiveFrom = new DateTime(2020, 1, 1),
                IsActive = true,
                RowVersion = new byte[] { 1 }
            });

            var c1 = new MilkCollection
            {
                FarmerID = Farmer1Id,
                SocietyID = SocietyId,
                CollectionDate = DateTime.Today,
                Shift = Shift.Morning,
                Quantity = 10,
                FatPercent = 4.2m,
                SNF = 8.5m,
                CLR = 28,
                RatePerLitre = 40,
                Amount = 400,
                RecordedBy = operatorUser.Id,
                RowVersion = new byte[] { 1 }
            };
            var c2 = new MilkCollection
            {
                FarmerID = Farmer2Id,
                SocietyID = SocietyId,
                CollectionDate = DateTime.Today,
                Shift = Shift.Morning,
                Quantity = 8,
                FatPercent = 4.0m,
                SNF = 8.5m,
                CLR = 28,
                RatePerLitre = 40,
                Amount = 320,
                RecordedBy = operatorUser.Id,
                RowVersion = new byte[] { 1 }
            };
            db.MilkCollections.AddRange(c1, c2);
            await db.SaveChangesAsync();
            CollectionFarmer1Id = c1.CollectionID;
            CollectionFarmer2Id = c2.CollectionID;
        }

        async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();

        public HttpClient CreateClientNoRedirect() =>
            CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        public static async Task LoginAsync(HttpClient client, string email, string password)
        {
            var page = await client.GetAsync("/Account/Login");
            page.EnsureSuccessStatusCode();
            var html = await page.Content.ReadAsStringAsync();
            var token = AntiforgeryToken(html);
            var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Password"] = password,
                ["__RequestVerificationToken"] = token
            }));
            Assert.True((int)response.StatusCode is >= 200 and < 400, $"Login failed for {email}: {response.StatusCode}");
            if (response.Headers.Location?.ToString().Contains("/Account/Login", StringComparison.OrdinalIgnoreCase) == true)
            {
                Assert.Fail($"Login redirected back to login for {email}: {response.Headers.Location}");
            }
        }

        public static string AntiforgeryToken(string html)
        {
            var match = Regex.Match(html, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""", RegexOptions.IgnoreCase)
                        is { Success: true } m
                ? m
                : Regex.Match(html, @"value=""([^""]+)""[^>]*name=""__RequestVerificationToken""", RegexOptions.IgnoreCase);
            Assert.True(match.Success, "Antiforgery token not found on page.");
            return match.Groups[1].Value;
        }

        private static async Task<ApplicationUser> CreateUserAsync(
            UserManager<ApplicationUser> users, string email, string name, string role, int societyId)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                SocietyID = societyId,
                IsActive = true,
                MustChangePassword = false,
                EmailConfirmed = true
            };
            var result = await users.CreateAsync(user, Password);
            Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));
            await users.AddToRoleAsync(user, role);
            return user;
        }
    }

    public class HttpIntegrationTests : IClassFixture<DairyWebAppFactory>
    {
        private readonly DairyWebAppFactory _factory;

        public HttpIntegrationTests(DairyWebAppFactory factory) => _factory = factory;

        [Fact]
        public async Task Operator_is_forbidden_from_admin_pages_and_farmer_is_forbidden_from_operator_pages()
        {
            var operatorClient = _factory.CreateClientNoRedirect();
            await DairyWebAppFactory.LoginAsync(operatorClient, DairyWebAppFactory.OperatorEmail, DairyWebAppFactory.Password);
            var adminPage = await operatorClient.GetAsync("/Admin/Home");
            Assert.Equal(HttpStatusCode.Found, adminPage.StatusCode);
            Assert.Contains("/Account/AccessDenied", adminPage.Headers.Location?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);

            var farmerClient = _factory.CreateClientNoRedirect();
            await DairyWebAppFactory.LoginAsync(farmerClient, DairyWebAppFactory.FarmerEmail, DairyWebAppFactory.Password);
            var operatorPage = await farmerClient.GetAsync("/Operator/MilkCollection");
            Assert.Equal(HttpStatusCode.Found, operatorPage.StatusCode);
            Assert.Contains("/Account/AccessDenied", operatorPage.Headers.Location?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Farmer_cannot_download_another_farmers_receipt()
        {
            var client = _factory.CreateClientNoRedirect();
            await DairyWebAppFactory.LoginAsync(client, DairyWebAppFactory.FarmerEmail, DairyWebAppFactory.Password);
            var response = await client.GetAsync($"/Farmer/Collections/Receipt/{_factory.CollectionFarmer2Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Operator_collection_receipt_returns_pdf()
        {
            var client = _factory.CreateClient();
            await DairyWebAppFactory.LoginAsync(client, DairyWebAppFactory.OperatorEmail, DairyWebAppFactory.Password);
            var response = await client.GetAsync($"/Operator/MilkCollection/Receipt/{_factory.CollectionFarmer1Id}");
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            Assert.True(bytes.Length > 100);
            Assert.Equal((byte)'%', bytes[0]);
        }

        [Fact]
        public async Task Post_without_antiforgery_token_is_rejected()
        {
            var client = _factory.CreateClientNoRedirect();
            var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = DairyWebAppFactory.AdminEmail,
                ["Password"] = "ChangeMe!123"
            }));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Duplicate_collection_post_returns_the_existing_entry_message()
        {
            var client = _factory.CreateClient();
            await DairyWebAppFactory.LoginAsync(client, DairyWebAppFactory.OperatorEmail, DairyWebAppFactory.Password);
            var page = await client.GetAsync("/Operator/MilkCollection");
            var html = await page.Content.ReadAsStringAsync();
            var token = DairyWebAppFactory.AntiforgeryToken(html);
            var today = DateTime.Today.ToString("yyyy-MM-dd");

            var response = await client.PostAsync("/Operator/MilkCollection/Create", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Form.FarmerID"] = _factory.Farmer1Id.ToString(),
                ["Form.CollectionDate"] = today,
                ["Form.Shift"] = "Morning",
                ["Form.Quantity"] = "5",
                ["Form.FatPercent"] = "4.2",
                ["Form.SNF"] = "8.5",
                ["Form.CLR"] = "28"
            }));
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("already exists", body, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Admin_can_open_union_dashboard()
        {
            var client = _factory.CreateClient();
            await DairyWebAppFactory.LoginAsync(client, DairyWebAppFactory.AdminEmail, "ChangeMe!123");
            var response = await client.GetAsync("/Admin/Home");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains("Union", html, StringComparison.OrdinalIgnoreCase);
        }
    }
}
