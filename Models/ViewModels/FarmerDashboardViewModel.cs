using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels
{
    public class FarmerDashboardViewModel
    {
        public string FarmerCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public DateTime OverviewDate { get; set; } = DateTime.Today;
        public decimal TodayQuantity { get; set; }
        public decimal TodayAmount { get; set; }

        public decimal MonthlyQuantity { get; set; }
        public decimal MonthlyAverageFat { get; set; }

        public bool HasLatestSettlement { get; set; }
        public DateTime LatestSettlementPeriodStart { get; set; }
        public DateTime LatestSettlementPeriodEnd { get; set; }
        public decimal LatestSettlementNetAmount { get; set; }
        public SettlementStatus LatestSettlementStatus { get; set; }

        public decimal PendingFeedDeduction { get; set; }
        public decimal PendingMedicineDeduction { get; set; }
    }
}
