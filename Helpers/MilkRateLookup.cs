using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Helpers
{
    public static class MilkRateLookup
    {
        public static MilkRate? SelectApplicable(
            IEnumerable<MilkRate> rates,
            decimal fatPercent,
            decimal snf,
            decimal clr,
            DateTime collectionDate)
        {
            var date = collectionDate.Date;
            var eligible = rates
                .Where(r => r.IsActive && r.EffectiveFrom.Date <= date)
                .ToList();

            if (eligible.Count == 0)
            {
                return null;
            }

            var latestEffectiveFrom = eligible.Max(r => r.EffectiveFrom.Date);

            return eligible
                .Where(r => r.EffectiveFrom.Date == latestEffectiveFrom
                            && r.FatPercentFrom <= fatPercent
                            && fatPercent <= r.FatPercentTo
                            && r.SnfPercentFrom <= snf
                            && snf <= r.SnfPercentTo
                            && r.ClrFrom <= clr
                            && clr <= r.ClrTo)
                .OrderByDescending(r => r.FatPercentFrom)
                .ThenByDescending(r => r.SnfPercentFrom)
                .ThenByDescending(r => r.ClrFrom)
                .FirstOrDefault();
        }
    }
}
