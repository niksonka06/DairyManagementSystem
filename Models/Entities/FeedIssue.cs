using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.Entities
{
    // Not in the synopsis's table list explicitly — same gap flagged in
    // Stage 0/6 (FeedInventory as specified is a stock MASTER, not a
    // transaction log). Needed for farmer-wise deduction history, which
    // Stage 10 (Settlement) reads directly.
    public class FeedIssue
    {
        public int IssueID { get; set; }

        public int FeedItemID { get; set; }
        public FeedInventory? FeedItem { get; set; }

        public int FarmerID { get; set; }
        public Farmer? Farmer { get; set; }

        public int SocietyID { get; set; }
        public Society? Society { get; set; }

        // Denormalized from FeedItem.ItemType at issue time — lets Stage 10
        // sum "SUM(TotalCost) WHERE ItemType = Feed" vs Medicine directly on
        // this table, without joining back to FeedInventory (and without risk
        // of the sum changing if an item's ItemType were ever edited later).
        public ItemType ItemType { get; set; }

        public decimal Quantity { get; set; }

        // Snapshotted at issue time — same historical-integrity reasoning as
        // MilkCollection.RatePerLitre. A later price change on FeedInventory
        // must never retroactively change what a past issue cost.
        public decimal UnitPriceAtIssue { get; set; }
        public decimal TotalCost { get; set; }

        public DateTime IssueDate { get; set; }

        public int IssuedBy { get; set; } // FK -> ApplicationUser.Id
        public ApplicationUser? IssuedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Settlement locking — same pattern as MilkCollection, wired properly
        // in Stage 10.
        public bool IsLocked { get; set; } = false;
        public int? LockedBySettlementID { get; set; }
    }
}
