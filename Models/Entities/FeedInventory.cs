using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.Entities
{
    // Named FeedInventory to match the synopsis's table name exactly, even
    // though (per the synopsis's own field description: "Name of feed OR
    // MEDICINE item") it holds both Feed and Medicine items, distinguished
    // by ItemType.
    public class FeedInventory
    {
        public int FeedItemID { get; set; }

        public int SocietyID { get; set; }
        public Society? Society { get; set; }

        public ItemType ItemType { get; set; }

        public string FeedName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty; // e.g. "kg", "packet"
        public decimal PricePerUnit { get; set; }
        public decimal StockQuantity { get; set; } = 0;

        // Not in the synopsis's table, added to make the "low-stock alert"
        // feature (explicitly required in Functional Requirements) actually
        // possible — per-item so a small-pack item and a bulk item can have
        // different sensible thresholds. Defaults to 10, editable per item.
        public decimal LowStockThreshold { get; set; } = 10;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
