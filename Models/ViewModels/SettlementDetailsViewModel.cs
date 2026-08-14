using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels
{
    public class SettlementDetailsViewModel
    {
        public int PaymentID { get; set; }
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public decimal GrossAmount { get; set; }
        public decimal FeedDeduction { get; set; }
        public decimal MedicineDeduction { get; set; }
        public decimal OtherDeductionsTotal { get; set; }
        public decimal PreviousDue { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal NetAmount { get; set; }

        public SettlementStatus Status { get; set; }
        public DateTime? GeneratedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? CancellationReason { get; set; }

        public List<(DeductionType Type, decimal Amount)> DeductionLines { get; set; } = new();

        public byte[]? RowVersion { get; set; }
    }
}
