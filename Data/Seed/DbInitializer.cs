using DairyManagementSystem.Data;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Data.Seed
{
    // Called once from Program.cs on startup. Idempotent — safe to run every
    // time the app starts, because every step checks "does this already
    // exist?" before creating anything. This is what lets a fresh clone of
    // the repo, pointed at a fresh database, become usable with zero manual
    // SQL — exactly what "seed roles / seed initial Admin user" in the
    // synopsis's Phase 1 blueprint calls for.
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var config = services.GetRequiredService<IConfiguration>();

            foreach (var roleName in Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            const string adminEmail = "admin@dairysystem.local";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin is null)
            {
                // Real deployments must override this via configuration/User
                // Secrets rather than trust this literal — see appsettings
                // "SeedAdminPassword" note below. Never ship a hardcoded
                // production password.
                var seedPassword = config["AppSettings:SeedAdminPassword"] ?? "ChangeMe!123";

                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    SocietyID = null, // Admin is unscoped — not tied to one society
                    IsActive = true,
                    MustChangePassword = true, // forces a real password on first login
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, seedPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Roles.Admin);
                }
                else
                {
                    // Fail loudly at startup rather than silently having no
                    // usable Admin account — that would lock you out of the
                    // whole system with no way back in.
                    throw new InvalidOperationException(
                        "Failed to seed initial Admin user: " +
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }

            var db = services.GetRequiredService<ApplicationDbContext>();
            await SeedMilkRateChartAsync(db);
        }

        // Fixed past date so collection tests on any recent day still match.
        // Used as the idempotency marker — do not change without a data reset.
        private static readonly DateTime SeedChartEffectiveFrom = new(2020, 1, 1);

        private static async Task SeedMilkRateChartAsync(ApplicationDbContext db)
        {
            var societyIds = await db.Societies.AsNoTracking()
                .Select(s => s.SocietyID)
                .ToListAsync();

            if (societyIds.Count == 0)
            {
                return;
            }

            var changed = false;

            foreach (var societyId in societyIds)
            {
                var placeholders = await db.MilkRates
                    .Where(r => r.SocietyID == societyId
                                && r.IsActive
                                && r.SnfPercentFrom == 7.5m
                                && r.SnfPercentTo == 11.0m
                                && r.ClrFrom == 0m
                                && r.ClrTo == 50m)
                    .ToListAsync();

                foreach (var placeholder in placeholders)
                {
                    placeholder.IsActive = false;
                    changed = true;
                }

                var alreadySeeded = await db.MilkRates.AnyAsync(r =>
                    r.SocietyID == societyId && r.EffectiveFrom == SeedChartEffectiveFrom);

                if (alreadySeeded)
                {
                    continue;
                }

                db.MilkRates.AddRange(BuildRealisticRateChart(societyId));
                changed = true;
            }

            if (changed)
            {
                await db.SaveChangesAsync();
            }
        }

        // Typical village-society chart: fat steps of 0.5%, SNF low/mid/high,
        // CLR low/normal/high. Rate = fat base + SNF adj + CLR adj.
        // Example: fat 4.50, SNF 8.20, CLR 28 → ₹44.
        public static List<MilkRate> BuildRealisticRateChart(int societyId)
        {
            var fatBands = new (decimal From, decimal To, decimal Base)[]
            {
                (2.50m, 2.99m, 28m),
                (3.00m, 3.49m, 32m),
                (3.50m, 3.99m, 36m),
                (4.00m, 4.49m, 40m),
                (4.50m, 4.99m, 44m),
                (5.00m, 5.49m, 48m),
                (5.50m, 5.99m, 52m),
                (6.00m, 6.49m, 56m),
                (6.50m, 7.49m, 62m),
                (7.50m, 9.00m, 70m)
            };

            var snfBands = new (decimal From, decimal To, decimal Adj)[]
            {
                (7.50m, 8.19m, -2m),
                (8.20m, 8.69m, 0m),
                (8.70m, 11.00m, 3m)
            };

            var clrBands = new (decimal From, decimal To, decimal Adj)[]
            {
                (20.00m, 26.99m, -2m),
                (27.00m, 30.99m, 0m),
                (31.00m, 40.00m, 2m)
            };

            var rates = new List<MilkRate>(fatBands.Length * snfBands.Length * clrBands.Length);

            foreach (var fat in fatBands)
            {
                foreach (var snf in snfBands)
                {
                    foreach (var clr in clrBands)
                    {
                        rates.Add(new MilkRate
                        {
                            SocietyID = societyId,
                            FatPercentFrom = fat.From,
                            FatPercentTo = fat.To,
                            SnfPercentFrom = snf.From,
                            SnfPercentTo = snf.To,
                            ClrFrom = clr.From,
                            ClrTo = clr.To,
                            RatePerLitre = fat.Base + snf.Adj + clr.Adj,
                            EffectiveFrom = SeedChartEffectiveFrom,
                            IsActive = true
                        });
                    }
                }
            }

            return rates;
        }
    }
}
