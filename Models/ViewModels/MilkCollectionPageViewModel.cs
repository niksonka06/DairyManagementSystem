using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels
{
    public class MilkCollectionPageViewModel
    {
        public DateTime SelectedDate { get; set; }
        public decimal DayTotalQuantity { get; set; }
        public decimal DayTotalAmount { get; set; }
        public MilkCollectionFormViewModel Form { get; set; } = new();
        public PagedTable<MilkCollectionListItemViewModel> Collections { get; set; } = new();
    }
}
