using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class OperatorFormViewModel
    {
        public int UserId { get; set; } // 0 on Create

        [StringLength(20)]
        [Display(Name = "Operator Code")]
        public string StaffCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Society")]
        public int SocietyID { get; set; }

        public List<SocietyListItemViewModel> AvailableSocieties { get; set; } = new();
    }
}
