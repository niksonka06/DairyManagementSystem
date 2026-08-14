using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.Entities
{
    // Class named Payment to match the synopsis's table name ("Payments")
    // exactly. Everywhere else in the app (controllers, views, business
    // language) this is called a "Settlement" — the synopsis itself uses
    // both terms interchangeably ("Weekly Settlement" module, "Payments" table).
    public class Payment
    {
        public int PaymentID { get; set; }

        public int FarmerID { get; set; }
        public Farmer? Farmer { get; set; }

        public int SocietyID { get; set; }
        public Society? Society { get; set; }

        public DateTime PeriodStart { get; set; } // always a Monday
        public DateTime PeriodEnd { get; set; }   // always the following Sunday

        // Computed from unlocked MilkCollections/FeedIssues at Draft-creation
        // time (and refreshable via Recalculate before Generate). Snapshotted
        // permanently once Generated.
        public decimal GrossAmount { get; set; }
        public decimal FeedDeduction { get; set; }
        public decimal MedicineDeduction { get; set; }

        // Sum of this settlement's SettlementDeduction child rows — stored
        // for quick display, always kept in sync by the service, not
        // independently editable.
        public decimal OtherDeductionsTotal { get; set; }

        public decimal PreviousDue { get; set; }
        public decimal AdvancePaid { get; set; }

        // GrossAmount - FeedDeduction - MedicineDeduction - OtherDeductionsTotal + PreviousDue - AdvancePaid
        public decimal NetAmount { get; set; }

        public SettlementStatus Status { get; set; } = SettlementStatus.Draft;

        public int? GeneratedBy { get; set; } // FK -> ApplicationUser.Id, set only when Generate is actually clicked
        public ApplicationUser? GeneratedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? GeneratedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        // Cancellation audit trail — required whenever a Generated settlement
        // is cancelled (unlocking its collections), per the synopsis's
        // "unlocking requires a reason" rule.
        public string? CancellationReason { get; set; }
        public int? CancelledBy { get; set; }
        public DateTime? CancelledAt { get; set; }

        public List<SettlementDeduction> Deductions { get; set; } = new();

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
