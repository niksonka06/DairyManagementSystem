namespace DairyManagementSystem.Models.ViewModels.Reports
{
    public class DailyReconciliationRow
    {
        public DateTime Date { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalDispatched { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercent { get; set; }
        public string? VarianceReason { get; set; }
        public string? VehicleNo { get; set; }
        public string? Destination { get; set; }
        public bool HasDispatch { get; set; }
    }

    public class DailyReconciliationReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; } = DateTime.Today;
        public List<DailyReconciliationRow> Rows { get; set; } = new();
    }
}
