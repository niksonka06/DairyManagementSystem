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
        public DbSet<Farmer> Farmers => Set<Farmer>();
        public DbSet<MilkRate> MilkRates => Set<MilkRate>();
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

            builder.Entity<Farmer>(entity =>
            {
                entity.Property(f => f.FarmerCode).HasMaxLength(20).IsRequired();
                entity.Property(f => f.FullName).HasMaxLength(100).IsRequired();
                entity.Property(f => f.Phone).HasMaxLength(15).IsRequired();
                entity.Property(f => f.Address).HasMaxLength(300).IsRequired();
                entity.Property(f => f.BankAccountNo).HasMaxLength(20).IsRequired();
                entity.Property(f => f.BankName).HasMaxLength(100).IsRequired();
                entity.Property(f => f.IFSC).HasMaxLength(11).IsRequired();

                // FarmerCode is unique PER SOCIETY (not globally) — a composite
                // unique index matches that business rule exactly, versus a
                // single-column unique index which would be too strict.
                entity.HasIndex(f => new { f.SocietyID, f.FarmerCode }).IsUnique();

                entity.HasIndex(f => f.UserID).IsUnique(); // one login per farmer

                entity.HasOne(f => f.Society)
                    .WithMany()
                    .HasForeignKey(f => f.SocietyID)
                    .OnDelete(DeleteBehavior.Restrict); // never cascade-delete a society and silently wipe its farmers

                entity.HasOne(f => f.User)
                    .WithMany()
                    .HasForeignKey(f => f.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(f => f.RowVersion).IsRowVersion();
            });

            builder.Entity<MilkRate>(entity =>
            {
                entity.HasKey(r => r.RateID);
                entity.Property(r => r.FatPercent).HasColumnType("decimal(4,2)");
                entity.Property(r => r.RatePerLitre).HasColumnType("decimal(8,2)");
                entity.Property(r => r.EffectiveFrom).HasColumnType("date");

                // CHECK constraints at the database level — matches the
                // synopsis's explicit CHECK(2.5-9.0) / CHECK(>0), and protects
                // data integrity even if a future code path (or a raw SQL
                // script) bypasses the C# [Range] validation.
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_FatPercent", "[FatPercent] BETWEEN 2.5 AND 9.0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_RatePerLitre", "[RatePerLitre] > 0"));

                // One rate per (society, fat%, effective date) — prevents two
                // ambiguous rows for the exact same band on the exact same day.
                entity.HasIndex(r => new { r.SocietyID, r.FatPercent, r.EffectiveFrom }).IsUnique();

                entity.HasOne(r => r.Society)
                    .WithMany()
                    .HasForeignKey(r => r.SocietyID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(r => r.RowVersion).IsRowVersion();
            });

            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.LogID); // LogID doesn't match EF Core's "Id"/"{ClassName}Id" convention, so it must be declared explicitly
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
