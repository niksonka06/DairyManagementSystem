namespace DairyManagementSystem.Models.Entities
{
    public class AuditLog
    {
        public long LogID { get; set; }

        public string EntityType { get; set; } = string.Empty; // e.g. "Society", "Farmer"
        public int EntityID { get; set; }

        public string Action { get; set; } = string.Empty; // AuditAction.ToString()

        // JSON snapshots. Nullable because a Create has no OldValue, and some
        // actions (e.g. Deactivated) may not need a full NewValue snapshot.
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public int PerformedBy { get; set; } // FK -> ApplicationUser.Id
        public ApplicationUser? PerformedByUser { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
