namespace DairyManagementSystem.Models.ViewModels
{
    public class MilkRateListItemViewModel
    {
        public int RateID { get; set; }
        public decimal FatPercent { get; set; }
        public decimal RatePerLitre { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public bool IsActive { get; set; }
    }
}
