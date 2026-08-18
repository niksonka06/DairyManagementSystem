using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class FeedIssueFormViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Please select an item.")]
        [Display(Name = "Feed / Medicine Item")]
        public int FeedItemID { get; set; }

        [Required(ErrorMessage = "Please select a farmer.")]
        [Display(Name = "Farmer")]
        public int FarmerID { get; set; }

        [Required]
        [Range(0.5, 100000, ErrorMessage = "Quantity must be at least 0.5.")]
        [Display(Name = "Quantity")]
        public decimal? Quantity { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        public int SocietyID { get; set; }

        public List<FeedInventoryListItemViewModel> AvailableItems { get; set; } = new();
        public List<FarmerListItemViewModel> AvailableFarmers { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Quantity.HasValue)
            {
                var doubled = Quantity.Value * 2m;
                if (doubled != decimal.Truncate(doubled))
                {
                    yield return new ValidationResult(
                        "Quantity must be in steps of 0.5.",
                        new[] { nameof(Quantity) });
                }
            }
        }
    }
}
