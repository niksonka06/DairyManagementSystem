using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class DispatchFormViewModel
    {
        public int DispatchID { get; set; } // 0 on Create

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Dispatch Date")]
        public DateTime DispatchDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Dispatch Time")]
        public TimeSpan DispatchTime { get; set; } = DateTime.Now.TimeOfDay;

        [Required]
        [StringLength(20)]
        [Display(Name = "Vehicle No.")]
        public string VehicleNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000, ErrorMessage = "Total dispatched must be greater than 0.")]
        [Display(Name = "Total Dispatched (Litres)")]
        public decimal TotalDispatched { get; set; }

        [Display(Name = "Total Collected (Litres)")]
        public decimal TotalCollected { get; set; }

        [StringLength(300)]
        [Display(Name = "Variance Reason (required if variance is above threshold)")]
        public string? VarianceReason { get; set; }

        public int SocietyID { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
