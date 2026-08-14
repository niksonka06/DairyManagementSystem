namespace DairyManagementSystem.Models.ViewModels.Reports
{
    public class DispatchReportRow
    {
        public DateTime DispatchDate { get; set; }
        public string VehicleNo { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal TotalCollected { get; set; }
        public decimal TotalDispatched { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercent { get; set; }
        public string OperatorName { get; set; } = string.Empty;
    }

    public class DispatchReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; } = DateTime.Today;
        public List<DispatchReportRow> Rows { get; set; } = new();
    }
}
