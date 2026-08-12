using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.Entities
{
    public class MilkCollection
    {
        public int CollectionID { get; set; }

        public int FarmerID { get; set; }
        public Farmer? Farmer { get; set; }

        public int SocietyID { get; set; }
        public Society? Society { get; set; }

        public DateTime CollectionDate { get; set; } // DATE, no time component
        public Shift Shift { get; set; }

        public decimal Quantity { get; set; }     // litres, CHECK 0.5-500
        public decimal FatPercent { get; set; }   // CHECK 2.5-9.0
        public decimal? SNF { get; set; }          // CHECK 7.5-11.0, nullable
        public decimal? CLR { get; set; }          // nullable, no range constraint per synopsis

        // Snapshotted at insert time — NEVER recalculated later, even if the
        // MilkRates chart changes afterward. This is the historical-integrity
        // rule from Stage 0/6: a collection permanently keeps the rate that
        // was applicable the day it was recorded.
        public decimal RatePerLitre { get; set; }
        public decimal Amount { get; set; }

        public int RecordedBy { get; set; } // FK -> ApplicationUser.Id
        public ApplicationUser? RecordedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Settlement locking (full enforcement arrives in Stage 10, but the
        // columns exist from here so nothing needs retrofitting later).
        public bool IsLocked { get; set; } = false;
        public int? LockedBySettlementID { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
