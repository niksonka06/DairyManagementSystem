using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Helpers
{
    public static class SettlementKpis
    {
        public static decimal PayableGenerated(IEnumerable<Payment> generated)
        {
            return generated.Where(p => p.Status == SettlementStatus.Generated && p.NetAmount > 0).Sum(p => p.NetAmount);
        }

        public static decimal CarryForwardGenerated(IEnumerable<Payment> generated)
        {
            return generated.Where(p => p.Status == SettlementStatus.Generated && p.NetAmount < 0).Sum(p => p.NetAmount);
        }

        public static int PayableCount(IEnumerable<Payment> generated)
        {
            return generated.Count(p => p.Status == SettlementStatus.Generated && p.NetAmount > 0);
        }
    }
}
