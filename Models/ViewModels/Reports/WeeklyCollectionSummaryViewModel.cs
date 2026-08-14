namespace DairyManagementSystem.Models.ViewModels.Reports
{
    public class WeeklyCollectionRow
    {
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public decimal TotalQuantity { get; set; }
        public decimal AverageFat { get; set; }
        public decimal GrossAmount { get; set; }
    }

    public class WeeklyCollectionSummaryViewModel
    {
        public DateTime WeekReferenceDate { get; set; } = DateTime.Today;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<WeeklyCollectionRow> Rows { get; set; } = new();
    }
}
