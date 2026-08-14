namespace DairyManagementSystem.Models.ViewModels.Reports
{
    public class DailyCollectionReportViewModel
    {
        public DateTime ReportDate { get; set; } = DateTime.Today;

        public decimal TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public int FarmerCount { get; set; }
        public decimal AverageFatPercent { get; set; }

        public decimal MorningQuantity { get; set; }
        public decimal MorningAmount { get; set; }
        public decimal EveningQuantity { get; set; }
        public decimal EveningAmount { get; set; }
    }
}
