using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class SettlementUnlockListItemViewModel
    {
        public int PaymentID { get; set; }
        public string SocietyName { get; set; } = string.Empty;
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime? GeneratedAt { get; set; }
    }

    public class SettlementUnlockViewModel
    {
        public int PaymentID { get; set; }

        public byte[]? RowVersion { get; set; }

        [Required(ErrorMessage = "A reason is required to unlock a generated settlement.")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
