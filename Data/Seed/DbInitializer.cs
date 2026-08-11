using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.AspNetCore.Identity;

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
        }
    }
}
