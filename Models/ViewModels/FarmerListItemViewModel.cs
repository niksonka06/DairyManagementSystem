namespace DairyManagementSystem.Models.ViewModels
{
    public class FarmerListItemViewModel
    {
        public int FarmerID { get; set; }
        public string FarmerCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
