using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.ViewModels
{
    public class MilkCollectionPageViewModel
    {
        public DateTime SelectedDate { get; set; }
        public decimal DayTotalQuantity { get; set; }
        public decimal DayTotalAmount { get; set; }
        public decimal DayRejectedQuantity { get; set; }
        public bool MorningClosed { get; set; }
        public bool EveningClosed { get; set; }
        public bool CanReopenMorning { get; set; }
        public bool CanReopenEvening { get; set; }
        public MilkCollectionFormViewModel Form { get; set; } = new();
        public PagedTable<MilkCollectionListItemViewModel> Collections { get; set; } = new();

        // After morning is closed, keep the form on evening (and the reverse)
        // so the operator can still record the open shift without a reload.
        public static Shift PreferOpenFormShift(Shift requested, bool morningClosed, bool eveningClosed)
        {
            if (requested == Shift.Morning && morningClosed && !eveningClosed)
            {
                return Shift.Evening;
            }

            if (requested == Shift.Evening && eveningClosed && !morningClosed)
            {
                return Shift.Morning;
            }

            return requested;
        }
    }
}
