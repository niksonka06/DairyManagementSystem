using System.ComponentModel.DataAnnotations;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels
{
    public class MilkCollectionFormViewModel
    {
        public int CollectionID { get; set; } // 0 on Create

        [Required(ErrorMessage = "Please select a farmer.")]
        [Display(Name = "Farmer")]
        public int FarmerID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Collection Date")]
        public DateTime CollectionDate { get; set; } = DateTime.Today;

        [Required]
        public Shift Shift { get; set; }

        [Required]
        [Range(0.5, 500, ErrorMessage = "Quantity must be between 0.5 and 500 litres.")]
        [Display(Name = "Quantity (Litres)")]
        public decimal? Quantity { get; set; }

        [Required]
        [Range(2.5, 9.0, ErrorMessage = "Fat percentage must be between 2.5 and 9.0.")]
        [Display(Name = "Fat %")]
        public decimal? FatPercent { get; set; }

        [Required]
        [Range(7.5, 11.0, ErrorMessage = "SNF must be between 7.5 and 11.0.")]
        [Display(Name = "SNF")]
        public decimal? SNF { get; set; }

        [Required]
        [Range(0, 50, ErrorMessage = "CLR must be between 0 and 50.")]
        [Display(Name = "CLR")]
        public decimal? CLR { get; set; }

        public int SocietyID { get; set; } // derived server-side, never posted

        public byte[]? RowVersion { get; set; }

        public List<FarmerListItemViewModel> AvailableFarmers { get; set; } = new();
    }
}
