using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Models.Entities
{
    public class SettlementDeduction
    {
        public int SettlementDeductionID { get; set; }

        public int PaymentID { get; set; }
        public Payment? Payment { get; set; }

        public DeductionType DeductionType { get; set; }
        public decimal Amount { get; set; }
    }
}
