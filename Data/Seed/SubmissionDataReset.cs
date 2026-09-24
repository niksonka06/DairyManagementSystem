using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Data.Seed
{
    // Wipes every row (schema and migrations stay) and loads a small,
    // consistent demo dataset for project submission. Run with:
    //   dotnet run -- --reset-submission-data
    public static class SubmissionDataReset
    {
        public const string DemoPassword = "Dairy@123";

        public static async Task RunAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SubmissionDataReset");

            logger.LogWarning("Clearing DairyManagement and loading submission demo data.");

            await ClearAllRowsAsync(db);
            db.ChangeTracker.Clear();

            foreach (var roleName in Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            var kottayam = new Society
            {
                SocietyName = "St. Mary's Milk Producers Society, Ettumanoor",
                RegistrationNo = "KTM/MPS/0142",
                Address = "Ettumanoor, Kottayam, Kerala 686631",
                ContactPhone = "04812580142",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var palai = new Society
            {
                SocietyName = "Palai Dairy Cooperative Society",
                RegistrationNo = "KTM/MPS/0208",
                Address = "Palai, Kottayam, Kerala 686575",
                ContactPhone = "04822220108",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Societies.AddRange(kottayam, palai);
            await db.SaveChangesAsync();

            await CreateOperatorAsync(userManager, "Rajan K. Nair", "rajan.operator@dairysystem.local", "OPR001", kottayam.SocietyID);
            await CreateOperatorAsync(userManager, "Suresh Mathew", "suresh.operator@dairysystem.local", "OPR002", palai.SocietyID);

            await CreateFarmersAsync(db, userManager, kottayam.SocietyID, new (string Name, string Email, string Phone)[]
            {
                ("Anil Kumar", "anil.farmer@dairysystem.local", "9847011001"),
                ("Bindu Thomas", "bindu.farmer@dairysystem.local", "9847011002"),
                ("Jose Varghese", "jose.farmer@dairysystem.local", "9847011003"),
                ("Latha Mohan", "latha.farmer@dairysystem.local", "9847011004")
            });

            await CreateFarmersAsync(db, userManager, palai.SocietyID, new (string Name, string Email, string Phone)[]
            {
                ("Mini George", "mini.farmer@dairysystem.local", "9847022001"),
                ("Prakash Pillai", "prakash.farmer@dairysystem.local", "9847022002"),
                ("Reena Joseph", "reena.farmer@dairysystem.local", "9847022003"),
                ("Vinu Abraham", "vinu.farmer@dairysystem.local", "9847022004")
            });

            db.FeedInventoryItems.AddRange(
                Stock(kottayam.SocietyID, ItemType.Feed, "Cattle Feed (Pellet)", "kg", 32m, 500m, 50m),
                Stock(kottayam.SocietyID, ItemType.Feed, "Mineral Mixture", "kg", 85m, 80m, 10m),
                Stock(kottayam.SocietyID, ItemType.Medicine, "Dewormer Bolus", "packet", 45m, 40m, 8m),
                Stock(palai.SocietyID, ItemType.Feed, "Cattle Feed (Pellet)", "kg", 32m, 420m, 50m),
                Stock(palai.SocietyID, ItemType.Feed, "Mineral Mixture", "kg", 85m, 60m, 10m),
                Stock(palai.SocietyID, ItemType.Medicine, "Dewormer Bolus", "packet", 45m, 30m, 8m));
            await db.SaveChangesAsync();

            // Admin, rate charts, and 14 days of collections / dispatch / issues / advances.
            await DbInitializer.SeedAsync(services);

            var admin = await userManager.FindByEmailAsync("admin@dairysystem.local")
                ?? throw new InvalidOperationException("Admin user was not seeded.");
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
            var reset = await userManager.ResetPasswordAsync(admin, resetToken, DemoPassword);
            if (!reset.Succeeded)
            {
                throw new InvalidOperationException(
                    "Could not set the admin password: " +
                    string.Join("; ", reset.Errors.Select(e => e.Description)));
            }

            admin.MustChangePassword = false;
            admin.FullName = "System Administrator";
            var update = await userManager.UpdateAsync(admin);
            if (!update.Succeeded)
            {
                throw new InvalidOperationException(
                    "Could not update the admin account: " +
                    string.Join("; ", update.Errors.Select(e => e.Description)));
            }

            await SeedSampleSettlementsAsync(services, kottayam.SocietyID, logger);

            logger.LogInformation(
                "Submission data ready. Password for every account is {Password}. Admin: admin@dairysystem.local",
                DemoPassword);
        }

        private static async Task SeedSampleSettlementsAsync(IServiceProvider services, int societyId, ILogger logger)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var payments = services.GetRequiredService<IPaymentService>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var operatorUser = (await userManager.GetUsersInRoleAsync(Roles.Operator))
                .FirstOrDefault(o => o.SocietyID == societyId);
            if (operatorUser is null)
            {
                logger.LogWarning("Skipped sample settlements: no operator for society {SocietyId}.", societyId);
                return;
            }

            var farmers = await db.Farmers.AsNoTracking()
                .Where(f => f.SocietyID == societyId && f.IsActive)
                .OrderBy(f => f.FarmerCode)
                .Select(f => new { f.FarmerID, f.FullName })
                .ToListAsync();

            var previousWeek = DateTime.Today.AddDays(-7);
            var generated = 0;

            foreach (var farmer in farmers)
            {
                if (generated >= 2)
                {
                    break;
                }

                try
                {
                    var draft = await payments.CreateDraftAsync(new SettlementCreateViewModel
                    {
                        FarmerID = farmer.FarmerID,
                        SocietyID = societyId,
                        WeekReferenceDate = previousWeek,
                        SocietyFeeDeduction = generated == 0 ? 25m : 0m
                    }, operatorUser.Id);

                    var rowVersion = await CurrentRowVersionAsync(db, draft.PaymentID);
                    await payments.GenerateAsync(draft.PaymentID, societyId, operatorUser.Id, rowVersion);

                    if (generated == 0)
                    {
                        rowVersion = await CurrentRowVersionAsync(db, draft.PaymentID);
                        await payments.MarkPaidAsync(draft.PaymentID, societyId, operatorUser.Id, rowVersion);
                        logger.LogInformation("Marked last week's settlement Paid for {Farmer}.", farmer.FullName);
                    }
                    else
                    {
                        logger.LogInformation("Left last week's settlement Generated (unpaid) for {Farmer}.", farmer.FullName);
                    }

                    generated++;
                }
                catch (BusinessRuleException ex)
                {
                    logger.LogInformation("No settlement for {Farmer}: {Reason}", farmer.FullName, ex.Message);
                }
            }
        }

        private static async Task<byte[]> CurrentRowVersionAsync(ApplicationDbContext db, int paymentId)
        {
            db.ChangeTracker.Clear();
            var rowVersion = await db.Payments.AsNoTracking()
                .Where(p => p.PaymentID == paymentId)
                .Select(p => p.RowVersion)
                .FirstAsync();
            return rowVersion;
        }

        private static async Task CreateFarmersAsync(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            int societyId,
            (string Name, string Email, string Phone)[] people)
        {
            var n = 1;
            foreach (var person in people)
            {
                var user = await CreateUserAsync(
                    userManager,
                    person.Name,
                    person.Email,
                    Roles.Farmer,
                    societyId,
                    staffCode: null);

                db.Farmers.Add(new Farmer
                {
                    FarmerCode = $"F{n:000}",
                    FullName = person.Name,
                    Phone = person.Phone,
                    Address = "Kottayam, Kerala",
                    BankAccountNo = $"2010000{societyId}{n:000}",
                    BankName = "State Bank of India",
                    IFSC = "SBIN0004567",
                    SocietyID = societyId,
                    UserID = user.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                n++;
            }

            await db.SaveChangesAsync();
        }

        private static async Task CreateOperatorAsync(
            UserManager<ApplicationUser> userManager,
            string fullName,
            string email,
            string staffCode,
            int societyId)
        {
            await CreateUserAsync(userManager, fullName, email, Roles.Operator, societyId, staffCode);
        }

        private static async Task<ApplicationUser> CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string fullName,
            string email,
            string role,
            int? societyId,
            string? staffCode)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                SocietyID = societyId,
                StaffCode = staffCode,
                IsActive = true,
                MustChangePassword = false,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create {email}: " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign {role} to {email}: " +
                    string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }

            return user;
        }

        private static FeedInventory Stock(
            int societyId,
            ItemType itemType,
            string name,
            string unit,
            decimal price,
            decimal quantity,
            decimal lowStock)
        {
            return new FeedInventory
            {
                SocietyID = societyId,
                ItemType = itemType,
                FeedName = name,
                Unit = unit,
                PricePerUnit = price,
                StockQuantity = quantity,
                LowStockThreshold = lowStock,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static async Task ClearAllRowsAsync(ApplicationDbContext db)
        {
            await db.Database.ExecuteSqlRawAsync(@"
DECLARE @sql nvarchar(max) = N'';

SELECT @sql += N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name) + N' NOCHECK CONSTRAINT ALL;'
FROM sys.tables
WHERE name <> N'__EFMigrationsHistory';
EXEC sp_executesql @sql;

SET @sql = N'';
SELECT @sql += N'DELETE FROM ' + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name) + N';'
FROM sys.tables
WHERE name <> N'__EFMigrationsHistory';
EXEC sp_executesql @sql;

SET @sql = N'';
SELECT @sql += N'DBCC CHECKIDENT (''' + SCHEMA_NAME(t.schema_id) + N'.' + t.name + N''', RESEED, 0);'
FROM sys.tables t
WHERE t.name <> N'__EFMigrationsHistory'
  AND EXISTS (SELECT 1 FROM sys.identity_columns ic WHERE ic.object_id = t.object_id);
EXEC sp_executesql @sql;

SET @sql = N'';
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name) + N' WITH CHECK CHECK CONSTRAINT ALL;'
FROM sys.tables
WHERE name <> N'__EFMigrationsHistory';
EXEC sp_executesql @sql;
");
        }
    }
}
