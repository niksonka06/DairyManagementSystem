namespace DairyManagementSystem.Models.Entities
{
    public class Dispatch
    {
        public int DispatchID { get; set; }

        public int SocietyID { get; set; }
        public Society? Society { get; set; }

        public DateTime DispatchDate { get; set; } // date only
        public TimeSpan DispatchTime { get; set; }

        public string VehicleNo { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty; // destination dairy union

        // Snapshotted at save time from that day's MilkCollections — litres,
        // not rupees. Recomputed on every Edit save (not silently, the
        // operator explicitly triggers Edit to correct a mistake, so
        // refreshing this alongside is expected, unlike a farmer's locked
        // settlement amount).
        public decimal TotalCollected { get; set; }
        public decimal TotalDispatched { get; set; }

        public string? VarianceReason { get; set; }

        public int RecordedBy { get; set; }
        public ApplicationUser? RecordedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Not stored — always derived from the two snapshotted totals.
        public decimal Variance => TotalCollected - TotalDispatched;
        public decimal VariancePercent => TotalCollected == 0 ? 0 : Math.Abs(Variance) / TotalCollected * 100;
    }
}
