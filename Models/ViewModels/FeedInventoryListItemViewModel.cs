using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels
{
    public class FeedInventoryListItemViewModel
    {
        public int FeedItemID { get; set; }
        public ItemType ItemType { get; set; }
        public string FeedName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal PricePerUnit { get; set; }
        public decimal StockQuantity { get; set; }
        public decimal LowStockThreshold { get; set; }
        public bool IsActive { get; set; }

        public bool IsLowStock => StockQuantity <= LowStockThreshold;
    }
}
