using DairyManagementSystem.Helpers;
using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Tests
{
    public class MilkRateLookupTests
    {
        [Fact]
        public void SelectApplicable_uses_latest_effective_chart_and_matching_band()
        {
            var older = Band(1, new DateTime(2026, 1, 1), fatFrom: 3, fatTo: 5, rate: 30);
            var currentLow = Band(2, new DateTime(2026, 6, 1), fatFrom: 3, fatTo: 4.5m, rate: 38);
            var currentHigh = Band(3, new DateTime(2026, 6, 1), fatFrom: 4.6m, fatTo: 6, rate: 45);
            var inactive = Band(4, new DateTime(2026, 6, 1), fatFrom: 3, fatTo: 6, rate: 99, active: false);

            var match = MilkRateLookup.SelectApplicable(
                new[] { older, currentLow, currentHigh, inactive },
                fatPercent: 5.0m,
                snf: 8.5m,
                clr: 28m,
                collectionDate: new DateTime(2026, 8, 15));

            Assert.NotNull(match);
            Assert.Equal(3, match!.RateID);
            Assert.Equal(45m, match.RatePerLitre);
        }

        [Fact]
        public void SelectApplicable_returns_null_when_quality_is_outside_all_bands()
        {
            var rate = Band(1, new DateTime(2026, 1, 1), fatFrom: 3, fatTo: 4, rate: 35);

            var match = MilkRateLookup.SelectApplicable(
                new[] { rate },
                fatPercent: 8.0m,
                snf: 8.5m,
                clr: 28m,
                collectionDate: new DateTime(2026, 8, 15));

            Assert.Null(match);
        }

        private static MilkRate Band(int id, DateTime effectiveFrom, decimal fatFrom, decimal fatTo, decimal rate, bool active = true)
        {
            return new MilkRate
            {
                RateID = id,
                SocietyID = 1,
                FatPercentFrom = fatFrom,
                FatPercentTo = fatTo,
                SnfPercentFrom = 7.5m,
                SnfPercentTo = 11m,
                ClrFrom = 0,
                ClrTo = 50,
                RatePerLitre = rate,
                EffectiveFrom = effectiveFrom,
                IsActive = active
            };
        }
    }
}
