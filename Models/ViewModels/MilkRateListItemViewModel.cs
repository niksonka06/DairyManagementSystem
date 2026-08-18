namespace DairyManagementSystem.Models.ViewModels
{
    public class MilkRateListItemViewModel
    {
        public int RateID { get; set; }
        public decimal FatPercentFrom { get; set; }
        public decimal FatPercentTo { get; set; }
        public decimal SnfPercentFrom { get; set; }
        public decimal SnfPercentTo { get; set; }
        public decimal ClrFrom { get; set; }
        public decimal ClrTo { get; set; }
        public decimal RatePerLitre { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public bool IsActive { get; set; }
    }
}
