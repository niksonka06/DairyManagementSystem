using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels
{
    public class SettlementListItemViewModel
    {
        public int PaymentID { get; set; }
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal NetAmount { get; set; }
        public SettlementStatus Status { get; set; }
    }
}
