using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels
{
    public class FeedIssueListItemViewModel
    {
        public int IssueID { get; set; }
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public ItemType ItemType { get; set; }
        public string FeedName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime IssueDate { get; set; }
    }
}
