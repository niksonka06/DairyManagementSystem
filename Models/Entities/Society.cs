namespace DairyManagementSystem.Models.Entities
{
    public class Society
    {
        public int SocietyID { get; set; }

        public string SocietyName { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optimistic concurrency token. EF Core auto-updates this on every
        // save and includes it in the WHERE clause of UPDATE statements —
        // if another user changed the row first, this UPDATE affects 0 rows
        // and EF Core throws DbUpdateConcurrencyException instead of silently
        // overwriting their change.
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
