namespace DairyManagementSystem.Models.ViewModels
{
    public class OperatorDashboardViewModel
    {
        public DateTime OverviewDate { get; set; } = DateTime.Today;
        public decimal TodayMilkLitres { get; set; }
        public decimal TodayPayable { get; set; }
        public int ActiveFarmerCount { get; set; }
        public decimal PendingSettlementsAmount { get; set; }
        public int MorningEntries { get; set; }
        public int EveningEntries { get; set; }
        public List<string> TrendLabels { get; set; } = new();
        public List<decimal> TrendValues { get; set; } = new();
    }
}
