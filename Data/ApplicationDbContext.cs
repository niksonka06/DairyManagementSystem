using DairyManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Society> Societies => Set<Society>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        // Real business entities added here module by module as each stage
        // introduces them (Farmer in Stage 5, MilkRate in Stage 6, etc.).

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // must run first — sets up Identity's table mappings

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            });

            builder.Entity<Society>(entity =>
            {
                entity.Property(s => s.SocietyName).HasMaxLength(200).IsRequired();
                entity.Property(s => s.RegistrationNo).HasMaxLength(50).IsRequired();
                entity.Property(s => s.Address).HasMaxLength(300).IsRequired();
                entity.Property(s => s.ContactPhone).HasMaxLength(15).IsRequired();

                // Enforced at the database level too, not just the service-layer
                // check — belt and suspenders against race conditions where two
                // requests pass the uniqueness check at the same instant.
                entity.HasIndex(s => s.RegistrationNo).IsUnique();

                entity.Property(s => s.RowVersion).IsRowVersion();
            });

            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.LogID);
                entity.Property(a => a.EntityType).HasMaxLength(50).IsRequired();
                entity.Property(a => a.Action).HasMaxLength(50).IsRequired();

                entity.HasOne(a => a.PerformedByUser)
                    .WithMany()
                    .HasForeignKey(a => a.PerformedBy)
                    .OnDelete(DeleteBehavior.Restrict); // never cascade-delete audit history if a user is removed

                // Fast lookups by "show me the history for this entity" — the
                // access pattern every audit-log viewer (Stage 14) will use.
                entity.HasIndex(a => new { a.EntityType, a.EntityID });
            });
        }
    }
}
