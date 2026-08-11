namespace DairyManagementSystem.Models.ViewModels
{
    public class SocietyListItemViewModel
    {
        public int SocietyID { get; set; }
        public string SocietyName { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
