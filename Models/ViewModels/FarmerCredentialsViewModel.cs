namespace DairyManagementSystem.Models.ViewModels
{
    // Shown exactly once, immediately after farmer creation, so the operator
    // can note down and hand the temporary password to the farmer. The
    // password itself is never stored in plain text anywhere — only Identity's
    // hash is persisted — so if this screen is missed, the only recovery path
    // is an admin-triggered password reset, not retrieval.
    public class FarmerCredentialsViewModel
    {
        public int FarmerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string FarmerCode { get; set; } = string.Empty;
        public string LoginEmail { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}
