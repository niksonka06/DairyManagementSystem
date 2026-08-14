namespace DairyManagementSystem.Models.ViewModels
{
    public class DispatchListItemViewModel
    {
        public int DispatchID { get; set; }
        public DateTime DispatchDate { get; set; }
        public TimeSpan DispatchTime { get; set; }
        public string VehicleNo { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal TotalCollected { get; set; }
        public decimal TotalDispatched { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercent { get; set; }
        public string? VarianceReason { get; set; }
    }
}
