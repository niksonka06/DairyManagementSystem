using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class AdvancePaymentFormViewModel
    {
        [Required(ErrorMessage = "Please select a farmer.")]
        [Display(Name = "Farmer")]
        public int FarmerID { get; set; }

        [Required]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0.")]
        public decimal? Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        public int SocietyID { get; set; }

        public List<FarmerListItemViewModel> AvailableFarmers { get; set; } = new();
    }

    public class AdvancePaymentListItemViewModel
    {
        public int AdvancePaymentID { get; set; }
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public bool IsApplied { get; set; }
    }
}
