using System.ComponentModel.DataAnnotations;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels
{
    public class FeedInventoryFormViewModel
    {
        public int FeedItemID { get; set; } // 0 on Create

        [Required]
        public ItemType ItemType { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Item Name")]
        public string FeedName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Unit { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price Per Unit (₹)")]
        public decimal PricePerUnit { get; set; }

        [Required]
        [Range(0, 100000)]
        [Display(Name = "Low Stock Alert Threshold")]
        public decimal LowStockThreshold { get; set; } = 10;

        public int SocietyID { get; set; } // derived server-side

        public byte[]? RowVersion { get; set; }
    }
}
