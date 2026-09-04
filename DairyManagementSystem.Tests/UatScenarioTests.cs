using DairyManagementSystem.Helpers;

namespace DairyManagementSystem.Tests
{
    // Automated stand-ins for the synopsis Phase-6 UAT checks that can be
    // asserted without a browser. UI walkthroughs (operator records
    // collection, farmer downloads receipt) remain a manual checklist.
    public class UatScenarioTests
    {
        [Fact]
        public void UAT_collection_amount_is_qty_times_chart_rate()
        {
            Assert.Equal(200m, SettlementCalculator.ComputeCollectionAmount(5m, 40m));
        }

        [Fact]
        public void UAT_weekly_settlement_net_after_feed_medicine_other_advance_and_opening_balance()
        {
            var totals = SettlementCalculator.Compute(8500, 500, 150, 100, previousDue: 0, advancePaid: 250);
            Assert.Equal(7500m, totals.NetAmount);
            Assert.Equal(0m, totals.OpeningBalance);
            Assert.Equal(7500m, totals.ClosingBalance);
        }

        [Fact]
        public void UAT_negative_net_becomes_next_week_opening_balance()
        {
            var thisWeek = SettlementCalculator.Compute(400, 600, 0, 0, previousDue: 0, advancePaid: 0);
            Assert.Equal(-200m, thisWeek.ClosingBalance);

            var nextWeek = SettlementCalculator.Compute(1000, 0, 0, 0, previousDue: thisWeek.ClosingBalance, advancePaid: 0);
            Assert.Equal(-200m, nextWeek.OpeningBalance);
            Assert.Equal(800m, nextWeek.NetAmount);
        }
    }
}
