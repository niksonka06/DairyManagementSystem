namespace DairyManagementSystem.Models.Entities
{
    public class MilkRate
    {
        public int RateID { get; set; }

        public int SocietyID { get; set; }
        public Society? Society { get; set; }

        public decimal FatPercentFrom { get; set; }
        public decimal FatPercentTo { get; set; }

        public decimal SnfPercentFrom { get; set; }
        public decimal SnfPercentTo { get; set; }

        public decimal ClrFrom { get; set; }
        public decimal ClrTo { get; set; }

        public decimal RatePerLitre { get; set; }

        // DATE in SQL Server (no time component) — see OnModelCreating.
        public DateTime EffectiveFrom { get; set; }

        public bool IsActive { get; set; } = true;

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
