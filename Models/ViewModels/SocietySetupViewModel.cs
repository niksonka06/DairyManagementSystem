using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class SocietySetupViewModel : IValidatableObject
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Society Name")]
        public string SocietyName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Registration No.")]
        public string RegistrationNo { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        [Display(Name = "Contact Phone")]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;

        [Display(Name = "Also create the first operator account")]
        public bool CreateOperator { get; set; } = true;

        [StringLength(100)]
        [Display(Name = "Operator Full Name")]
        public string? OperatorFullName { get; set; }

        [EmailAddress]
        [Display(Name = "Operator Email")]
        public string? OperatorEmail { get; set; }

        public SocietyFormViewModel ToSocietyForm() => new()
        {
            SocietyName = SocietyName,
            RegistrationNo = RegistrationNo,
            Address = Address,
            ContactPhone = ContactPhone
        };

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!CreateOperator)
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(OperatorFullName))
            {
                yield return new ValidationResult(
                    "Operator name is required when creating an operator account.",
                    [nameof(OperatorFullName)]);
            }

            if (string.IsNullOrWhiteSpace(OperatorEmail))
            {
                yield return new ValidationResult(
                    "Operator email is required when creating an operator account.",
                    [nameof(OperatorEmail)]);
            }
        }
    }
}
