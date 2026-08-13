using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class AddStockViewModel
    {
        public int FeedItemID { get; set; }
        public string FeedName { get; set; } = string.Empty; // display only

        [Required]
        [Range(0.01, 100000, ErrorMessage = "Quantity to add must be greater than 0.")]
        [Display(Name = "Quantity to Add")]
        public decimal QuantityToAdd { get; set; }

        public int SocietyID { get; set; }
    }
}
