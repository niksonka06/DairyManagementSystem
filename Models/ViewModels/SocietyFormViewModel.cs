using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class SocietyFormViewModel
    {
        public int SocietyID { get; set; } // 0 on Create

        [Required]
        [StringLength(200)]
        [Display(Name = "Society Name")]
        public string SocietyName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Society Code")]
        public string RegistrationNo { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        [Display(Name = "Contact Phone")]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;

        // Hidden field on the Edit form — round-tripped so the service can
        // detect if someone else changed this row in between.
        public byte[]? RowVersion { get; set; }
    }
}
