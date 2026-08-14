using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels.Reports
{
    public class SettlementReportRow
    {
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal FeedDeduction { get; set; }
        public decimal MedicineDeduction { get; set; }
        public decimal OtherDeductionsTotal { get; set; }
        public decimal PreviousDue { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal NetAmount { get; set; }
        public SettlementStatus Status { get; set; }
    }

    public class FarmerSettlementReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; } = DateTime.Today;
        public List<SettlementReportRow> Rows { get; set; } = new();
        public decimal TotalNetAmount { get; set; }
    }
}
