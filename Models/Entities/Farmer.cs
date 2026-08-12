namespace DairyManagementSystem.Models.Entities
{
    public class Farmer
    {
        public int FarmerID { get; set; }

        // Society-scoped unique code (e.g. "SOC001-F001") — human-friendly
        // reference used on receipts, settlement slips, and as the basis for
        // the farmer's synthetic login email.
        public string FarmerCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string BankAccountNo { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;

        public int SocietyID { get; set; }
        public Society? Society { get; set; }

        // One login per farmer. Unique constraint enforced in OnModelCreating.
        public int UserID { get; set; }
        public ApplicationUser? User { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
