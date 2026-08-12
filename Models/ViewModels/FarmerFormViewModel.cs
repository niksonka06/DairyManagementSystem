using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class FarmerFormViewModel
    {
        public int FarmerID { get; set; } // 0 on Create

        [Required]
        [StringLength(20)]
        [Display(Name = "Farmer Code")]
        public string FarmerCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Bank Account No.")]
        public string BankAccountNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [StringLength(11)]
        public string IFSC { get; set; } = string.Empty;

        // Not shown on the form — the logged-in Operator's own SocietyID is
        // used automatically (see FarmerController). Never trust a SocietyID
        // coming from the browser for this operation, same reasoning as the
        // Farmer-Portal ownership rule, just applied one role up.
        public int SocietyID { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
