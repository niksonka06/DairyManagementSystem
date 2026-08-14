namespace DairyManagementSystem.Models.ViewModels.Reports
{
    public class FarmerFatRow
    {
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public decimal AverageFat { get; set; }
        public int CollectionCount { get; set; }
    }

    public class FatBucket
    {
        public string Label { get; set; } = string.Empty; // e.g. "3.5 - 4.5%"
        public int Count { get; set; }
    }

    public class FatTrendPoint
    {
        public DateTime Date { get; set; }
        public decimal AverageFat { get; set; }
    }

    public class FatAnalysisReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; } = DateTime.Today;

        public List<FarmerFatRow> FarmerAverages { get; set; } = new();
        public List<FatBucket> Distribution { get; set; } = new();
        public List<FatTrendPoint> Trend { get; set; } = new();
    }
}
