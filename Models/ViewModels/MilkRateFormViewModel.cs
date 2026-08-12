using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class MilkRateFormViewModel
    {
        public int RateID { get; set; } // 0 on Create

        [Required]
        [Range(2.5, 9.0, ErrorMessage = "Fat percentage must be between 2.5 and 9.0.")]
        [Display(Name = "Fat %")]
        public decimal FatPercent { get; set; }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Rate per litre must be greater than 0.")]
        [Display(Name = "Rate Per Litre (₹)")]
        public decimal RatePerLitre { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Effective From")]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        public int SocietyID { get; set; } // derived server-side, never posted

        public byte[]? RowVersion { get; set; }
    }
}
