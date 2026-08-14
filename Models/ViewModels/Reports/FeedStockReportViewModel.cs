using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels.Reports
{
    public class FeedStockRow
    {
        public ItemType ItemType { get; set; }
        public string FeedName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal StockQuantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public bool IsLowStock { get; set; }
    }

    public class FeedIssueRow
    {
        public DateTime IssueDate { get; set; }
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public ItemType ItemType { get; set; }
        public string FeedName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class FarmerDeductionTotalRow
    {
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public decimal TotalFeedCost { get; set; }
        public decimal TotalMedicineCost { get; set; }
    }

    public class FeedStockReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; } = DateTime.Today;

        public List<FeedStockRow> CurrentStock { get; set; } = new();
        public List<FeedIssueRow> RecentIssues { get; set; } = new();
        public List<FarmerDeductionTotalRow> FarmerDeductionTotals { get; set; } = new();
    }
}
