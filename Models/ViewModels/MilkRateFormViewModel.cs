using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class MilkRateFormViewModel : IValidatableObject
    {
        public int RateID { get; set; } // 0 on Create

        [Required]
        [Range(2.5, 9.0, ErrorMessage = "Fat from must be between 2.5 and 9.0.")]
        [Display(Name = "Fat % from")]
        public decimal? FatPercentFrom { get; set; }

        [Required]
        [Range(2.5, 9.0, ErrorMessage = "Fat to must be between 2.5 and 9.0.")]
        [Display(Name = "Fat % to")]
        public decimal? FatPercentTo { get; set; }

        [Required]
        [Range(7.5, 11.0, ErrorMessage = "SNF from must be between 7.5 and 11.0.")]
        [Display(Name = "SNF from")]
        public decimal? SnfPercentFrom { get; set; }

        [Required]
        [Range(7.5, 11.0, ErrorMessage = "SNF to must be between 7.5 and 11.0.")]
        [Display(Name = "SNF to")]
        public decimal? SnfPercentTo { get; set; }

        [Required]
        [Range(0, 50, ErrorMessage = "CLR from must be between 0 and 50.")]
        [Display(Name = "CLR from")]
        public decimal? ClrFrom { get; set; }

        [Required]
        [Range(0, 50, ErrorMessage = "CLR to must be between 0 and 50.")]
        [Display(Name = "CLR to")]
        public decimal? ClrTo { get; set; }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Rate per litre must be greater than 0.")]
        [Display(Name = "Rate Per Litre (₹)")]
        public decimal? RatePerLitre { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Effective From")]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        public int SocietyID { get; set; } // derived server-side, never posted

        public byte[]? RowVersion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FatPercentFrom.HasValue && FatPercentTo.HasValue && FatPercentTo < FatPercentFrom)
            {
                yield return new ValidationResult(
                    "Fat % to must be greater than or equal to fat % from.",
                    new[] { nameof(FatPercentTo) });
            }

            if (SnfPercentFrom.HasValue && SnfPercentTo.HasValue && SnfPercentTo < SnfPercentFrom)
            {
                yield return new ValidationResult(
                    "SNF to must be greater than or equal to SNF from.",
                    new[] { nameof(SnfPercentTo) });
            }

            if (ClrFrom.HasValue && ClrTo.HasValue && ClrTo < ClrFrom)
            {
                yield return new ValidationResult(
                    "CLR to must be greater than or equal to CLR from.",
                    new[] { nameof(ClrTo) });
            }
        }
    }
}
