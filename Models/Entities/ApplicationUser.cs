using Microsoft.AspNetCore.Identity;

namespace DairyManagementSystem.Models.Entities
{
    // Identity already gives us: Id (int, per our IdentityDbContext<...,int> config),
    // Email, PasswordHash, and everything else needed for login/security.
    // We only add the fields the synopsis's Users table needs on top of that.
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;

        // Nullable: Admin may be unscoped (not tied to one society).
        public int? SocietyID { get; set; }
        public Society? Society { get; set; }

        public bool IsActive { get; set; } = true;

        // Forces password change on first login. Defaults to true for every
        // new account — the seeded Admin's own seed logic will flip this
        // after you set a real password (see Data/Seed/DbInitializer.cs).
        public bool MustChangePassword { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
