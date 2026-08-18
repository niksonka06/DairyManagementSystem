using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels
{
    public class FarmerCollectionRowViewModel
    {
        public DateTime CollectionDate { get; set; }
        public Shift Shift { get; set; }
        public decimal Quantity { get; set; }
        public decimal FatPercent { get; set; }
        public decimal? SNF { get; set; }
        public decimal? CLR { get; set; }
        public decimal RatePerLitre { get; set; }
        public decimal Amount { get; set; }
        public bool IsLocked { get; set; }
    }
}
