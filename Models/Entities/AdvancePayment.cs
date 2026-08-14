namespace DairyManagementSystem.Models.Entities
{
    // Not in the synopsis's table list — required to support "farmer can
    // receive an advance mid-cycle that reduces their NEXT settlement"
    // (business decision made at Stage 10 kickoff).
    public class AdvancePayment
    {
        public int AdvancePaymentID { get; set; }

        public int FarmerID { get; set; }
        public Farmer? Farmer { get; set; }

        public int SocietyID { get; set; }

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        public int RecordedBy { get; set; }
        public ApplicationUser? RecordedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Once pulled into a Draft settlement and that settlement is
        // Generated, this becomes true and AppliedToPaymentID is set — an
        // applied advance can never be pulled into a second settlement.
        public bool IsApplied { get; set; } = false;
        public int? AppliedToPaymentID { get; set; }
    }
}
