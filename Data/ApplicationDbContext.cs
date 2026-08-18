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
        public DbSet<MilkCollection> MilkCollections => Set<MilkCollection>();
        public DbSet<FeedInventory> FeedInventoryItems => Set<FeedInventory>();
        public DbSet<FeedIssue> FeedIssues => Set<FeedIssue>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<SettlementDeduction> SettlementDeductions => Set<SettlementDeduction>();
        public DbSet<AdvancePayment> AdvancePayments => Set<AdvancePayment>();
        public DbSet<Dispatch> Dispatches => Set<Dispatch>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        // Real business entities added here module by module as each stage
        // introduces them (Farmer in Stage 5, MilkRate in Stage 6, etc.).

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // must run first — sets up Identity's table mappings

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();

                entity.HasOne(u => u.Society)
                    .WithMany()
                    .HasForeignKey(u => u.SocietyID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Society>(entity =>
            {
                entity.HasKey(s => s.SocietyID);
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
                entity.HasKey(f => f.FarmerID);
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
                entity.HasKey(r => r.RateID); // RateID doesn't match EF Core's "Id"/"{ClassName}Id" convention
                entity.Property(r => r.FatPercentFrom).HasColumnType("decimal(4,2)");
                entity.Property(r => r.FatPercentTo).HasColumnType("decimal(4,2)");
                entity.Property(r => r.SnfPercentFrom).HasColumnType("decimal(4,2)");
                entity.Property(r => r.SnfPercentTo).HasColumnType("decimal(4,2)");
                entity.Property(r => r.ClrFrom).HasColumnType("decimal(5,2)");
                entity.Property(r => r.ClrTo).HasColumnType("decimal(5,2)");
                entity.Property(r => r.RatePerLitre).HasColumnType("decimal(8,2)");
                entity.Property(r => r.EffectiveFrom).HasColumnType("date");

                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_FatPercentFrom", "[FatPercentFrom] BETWEEN 2.5 AND 9.0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_FatPercentTo", "[FatPercentTo] BETWEEN 2.5 AND 9.0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_FatRange", "[FatPercentFrom] <= [FatPercentTo]"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_SnfPercentFrom", "[SnfPercentFrom] BETWEEN 7.5 AND 11.0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_SnfPercentTo", "[SnfPercentTo] BETWEEN 7.5 AND 11.0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_SnfRange", "[SnfPercentFrom] <= [SnfPercentTo]"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_ClrFrom", "[ClrFrom] BETWEEN 0 AND 50"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_ClrTo", "[ClrTo] BETWEEN 0 AND 50"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_ClrRange", "[ClrFrom] <= [ClrTo]"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkRates_RatePerLitre", "[RatePerLitre] > 0"));

                entity.HasIndex(r => new { r.SocietyID, r.EffectiveFrom });

                entity.HasOne(r => r.Society)
                    .WithMany()
                    .HasForeignKey(r => r.SocietyID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(r => r.RowVersion).IsRowVersion();
            });

            builder.Entity<MilkCollection>(entity =>
            {
                entity.HasKey(c => c.CollectionID); // CollectionID doesn't match EF Core's convention

                entity.Property(c => c.CollectionDate).HasColumnType("date");
                entity.Property(c => c.Quantity).HasColumnType("decimal(8,2)");
                entity.Property(c => c.FatPercent).HasColumnType("decimal(4,2)");
                entity.Property(c => c.SNF).HasColumnType("decimal(4,2)");
                entity.Property(c => c.CLR).HasColumnType("decimal(5,2)");
                entity.Property(c => c.RatePerLitre).HasColumnType("decimal(8,2)");
                entity.Property(c => c.Amount).HasColumnType("decimal(10,2)");

                // Store the Shift enum as its string name ("Morning"/"Evening")
                // rather than an int — matches the synopsis's own
                // NVARCHAR(10) CHECK IN ('Morning','Evening') column exactly,
                // and keeps the raw table human-readable if inspected directly.
                entity.Property(c => c.Shift).HasConversion<string>().HasMaxLength(10);

                entity.ToTable(t => t.HasCheckConstraint("CK_MilkCollections_Quantity", "[Quantity] BETWEEN 0.5 AND 500"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkCollections_FatPercent", "[FatPercent] BETWEEN 2.5 AND 9.0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkCollections_SNF", "[SNF] IS NULL OR [SNF] BETWEEN 7.5 AND 11.0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_MilkCollections_RatePerLitre", "[RatePerLitre] > 0"));

                // One entry per farmer/date/shift — the duplicate-prevention
                // rule enforced at the database level too, not just the
                // service-layer check.
                entity.HasIndex(c => new { c.FarmerID, c.CollectionDate, c.Shift }).IsUnique();

                entity.HasOne(c => c.Farmer)
                    .WithMany()
                    .HasForeignKey(c => c.FarmerID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Society)
                    .WithMany()
                    .HasForeignKey(c => c.SocietyID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.RecordedByUser)
                    .WithMany()
                    .HasForeignKey(c => c.RecordedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(c => c.RowVersion).IsRowVersion();
            });

            builder.Entity<FeedInventory>(entity =>
            {
                entity.HasKey(f => f.FeedItemID); // FeedItemID doesn't match EF Core's naming convention
                entity.Property(f => f.ItemType).HasConversion<string>().HasMaxLength(10);
                entity.Property(f => f.FeedName).HasMaxLength(100).IsRequired();
                entity.Property(f => f.Unit).HasMaxLength(20).IsRequired();
                entity.Property(f => f.PricePerUnit).HasColumnType("decimal(10,2)");
                entity.Property(f => f.StockQuantity).HasColumnType("decimal(10,2)");
                entity.Property(f => f.LowStockThreshold).HasColumnType("decimal(10,2)");

                entity.ToTable(t => t.HasCheckConstraint("CK_FeedInventory_PricePerUnit", "[PricePerUnit] > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_FeedInventory_StockQuantity", "[StockQuantity] >= 0")); // never negative — the synopsis's core Feed rule

                entity.HasIndex(f => new { f.SocietyID, f.ItemType, f.FeedName }).IsUnique();

                entity.HasOne(f => f.Society)
                    .WithMany()
                    .HasForeignKey(f => f.SocietyID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(f => f.RowVersion).IsRowVersion();
            });

            builder.Entity<FeedIssue>(entity =>
            {
                entity.HasKey(i => i.IssueID); // IssueID doesn't match "FeedIssueId" convention
                entity.Property(i => i.ItemType).HasConversion<string>().HasMaxLength(10);
                entity.Property(i => i.Quantity).HasColumnType("decimal(10,2)");
                entity.Property(i => i.UnitPriceAtIssue).HasColumnType("decimal(10,2)");
                entity.Property(i => i.TotalCost).HasColumnType("decimal(10,2)");
                entity.Property(i => i.IssueDate).HasColumnType("date");

                entity.ToTable(t => t.HasCheckConstraint("CK_FeedIssues_Quantity", "[Quantity] > 0"));

                entity.HasOne(i => i.FeedItem)
                    .WithMany()
                    .HasForeignKey(i => i.FeedItemID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Farmer)
                    .WithMany()
                    .HasForeignKey(i => i.FarmerID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Society)
                    .WithMany()
                    .HasForeignKey(i => i.SocietyID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.IssuedByUser)
                    .WithMany()
                    .HasForeignKey(i => i.IssuedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.PaymentID);
                entity.Property(p => p.PeriodStart).HasColumnType("date");
                entity.Property(p => p.PeriodEnd).HasColumnType("date");
                entity.Property(p => p.GrossAmount).HasColumnType("decimal(12,2)");
                entity.Property(p => p.FeedDeduction).HasColumnType("decimal(12,2)");
                entity.Property(p => p.MedicineDeduction).HasColumnType("decimal(12,2)");
                entity.Property(p => p.OtherDeductionsTotal).HasColumnType("decimal(12,2)");
                entity.Property(p => p.PreviousDue).HasColumnType("decimal(12,2)");
                entity.Property(p => p.AdvancePaid).HasColumnType("decimal(12,2)");
                entity.Property(p => p.NetAmount).HasColumnType("decimal(12,2)");
                entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(p => p.CancellationReason).HasMaxLength(500);

                // Duplicate-settlement prevention at the DB level: one
                // NON-CANCELLED settlement per farmer per period. A filtered
                // unique index (not a plain one) is required here specifically
                // because Cancelled settlements must NOT count — a farmer can
                // have a cancelled settlement AND a fresh valid one for the
                // same week.
                entity.HasIndex(p => new { p.FarmerID, p.PeriodStart })
                    .IsUnique()
                    .HasFilter("[Status] <> 'Cancelled'");

                entity.HasOne(p => p.Farmer)
                    .WithMany()
                    .HasForeignKey(p => p.FarmerID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Society)
                    .WithMany()
                    .HasForeignKey(p => p.SocietyID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.GeneratedByUser)
                    .WithMany()
                    .HasForeignKey(p => p.GeneratedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.Deductions)
                    .WithOne(d => d.Payment)
                    .HasForeignKey(d => d.PaymentID)
                    .OnDelete(DeleteBehavior.Cascade); // deduction lines are owned by their settlement — delete with it (settlements are soft-cancelled, never hard-deleted, so this rarely fires)

                entity.Property(p => p.RowVersion).IsRowVersion();
            });

            builder.Entity<SettlementDeduction>(entity =>
            {
                entity.HasKey(d => d.SettlementDeductionID);
                entity.Property(d => d.DeductionType).HasConversion<string>().HasMaxLength(20);
                entity.Property(d => d.Amount).HasColumnType("decimal(12,2)");
                entity.ToTable(t => t.HasCheckConstraint("CK_SettlementDeductions_Amount", "[Amount] > 0"));
            });

            builder.Entity<AdvancePayment>(entity =>
            {
                entity.HasKey(a => a.AdvancePaymentID);
                entity.Property(a => a.Amount).HasColumnType("decimal(12,2)");
                entity.Property(a => a.PaymentDate).HasColumnType("date");
                entity.ToTable(t => t.HasCheckConstraint("CK_AdvancePayments_Amount", "[Amount] > 0"));

                entity.HasOne(a => a.Farmer)
                    .WithMany()
                    .HasForeignKey(a => a.FarmerID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.RecordedByUser)
                    .WithMany()
                    .HasForeignKey(a => a.RecordedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Dispatch>(entity =>
            {
                entity.HasKey(d => d.DispatchID); // matches convention, but declared explicitly for consistency

                entity.Property(d => d.DispatchDate).HasColumnType("date");
                entity.Property(d => d.DispatchTime).HasColumnType("time");
                entity.Property(d => d.VehicleNo).HasMaxLength(20).IsRequired();
                entity.Property(d => d.Destination).HasMaxLength(100).IsRequired();
                entity.Property(d => d.TotalCollected).HasColumnType("decimal(10,2)");
                entity.Property(d => d.TotalDispatched).HasColumnType("decimal(10,2)");
                entity.Property(d => d.VarianceReason).HasMaxLength(300);

                entity.ToTable(t => t.HasCheckConstraint("CK_Dispatches_TotalDispatched", "[TotalDispatched] > 0"));

                // One reconciliation record per society per day.
                entity.HasIndex(d => new { d.SocietyID, d.DispatchDate }).IsUnique();

                // Variance and VariancePercent are computed C# properties,
                // not mapped columns — EF Core ignores them automatically
                // since they have no setter backing field, but being
                // explicit here avoids any ambiguity.
                entity.Ignore(d => d.Variance);
                entity.Ignore(d => d.VariancePercent);

                entity.HasOne(d => d.Society)
                    .WithMany()
                    .HasForeignKey(d => d.SocietyID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.RecordedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.RecordedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(d => d.RowVersion).IsRowVersion();
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
