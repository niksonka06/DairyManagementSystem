namespace DairyManagementSystem.Models.ViewModels
{
    public class OperatorListItemViewModel
    {
        public int UserId { get; set; }
        public string StaffCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SocietyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
