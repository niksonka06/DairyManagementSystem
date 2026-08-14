using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class SettlementCreateViewModel
    {
        [Required(ErrorMessage = "Please select a farmer.")]
        [Display(Name = "Farmer")]
        public int FarmerID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Any date within the target week")]
        public DateTime WeekReferenceDate { get; set; } = DateTime.Today;

        [Range(0, 1000000)]
        [Display(Name = "Previous Due (₹)")]
        public decimal PreviousDue { get; set; } = 0;

        [Range(0, 1000000)]
        [Display(Name = "Loan Deduction (₹)")]
        public decimal LoanDeduction { get; set; } = 0;

        [Range(0, 1000000)]
        [Display(Name = "Insurance Deduction (₹)")]
        public decimal InsuranceDeduction { get; set; } = 0;

        [Range(0, 1000000)]
        [Display(Name = "Society Fee Deduction (₹)")]
        public decimal SocietyFeeDeduction { get; set; } = 0;

        [Range(0, 1000000)]
        [Display(Name = "Other Deduction (₹)")]
        public decimal OtherDeduction { get; set; } = 0;

        public int SocietyID { get; set; }

        public List<FarmerListItemViewModel> AvailableFarmers { get; set; } = new();

        // Populated when re-showing the form after a suggestion lookup, so
        // the operator sees where the pre-filled PreviousDue came from.
        public decimal? SuggestedPreviousDue { get; set; }
        public decimal? UnappliedAdvanceTotal { get; set; }
    }
}
