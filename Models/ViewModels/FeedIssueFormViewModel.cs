using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class FeedIssueFormViewModel
    {
        [Required(ErrorMessage = "Please select an item.")]
        [Display(Name = "Feed / Medicine Item")]
        public int FeedItemID { get; set; }

        [Required(ErrorMessage = "Please select a farmer.")]
        [Display(Name = "Farmer")]
        public int FarmerID { get; set; }

        [Required]
        [Range(0.01, 100000, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        public int SocietyID { get; set; }

        public List<FeedInventoryListItemViewModel> AvailableItems { get; set; } = new();
        public List<FarmerListItemViewModel> AvailableFarmers { get; set; } = new();
    }
}
