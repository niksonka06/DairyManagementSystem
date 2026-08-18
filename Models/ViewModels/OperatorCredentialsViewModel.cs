namespace DairyManagementSystem.Models.ViewModels
{
    public class OperatorCredentialsViewModel
    {
        public string? StaffCode { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string LoginEmail { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}
