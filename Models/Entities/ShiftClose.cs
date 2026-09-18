using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.Entities
{
    // One row per society + date + shift. Presence of the row means that
    // shift is finalized: no new collections and no edits until it is reopened
    // (only allowed while none of that shift's collections are settlement-locked).
    public class ShiftClose
    {
        public int ShiftCloseID { get; set; }

        public int SocietyID { get; set; }
        public Society? Society { get; set; }

        public DateTime CollectionDate { get; set; }
        public Shift Shift { get; set; }

        public int ClosedBy { get; set; }
        public ApplicationUser? ClosedByUser { get; set; }

        public DateTime ClosedAt { get; set; } = DateTime.UtcNow;
    }
}
