using DairyManagementSystem.Helpers;

namespace DairyManagementSystem.Tests
{
    public class SettlementCalculatorTests
    {
        [Fact]
        public void Compute_uses_synopsis_net_formula_and_named_balances()
        {
            var totals = SettlementCalculator.Compute(
                gross: 8500,
                feedDeduction: 500,
                medicineDeduction: 150,
                otherDeductionsTotal: 100,
                previousDue: -200,
                advancePaid: 300);

            Assert.Equal(7250m, totals.NetAmount);
            Assert.Equal(-200m, totals.OpeningBalance);
            Assert.Equal(7250m, totals.ClosingBalance);
        }

        [Fact]
        public void ComputeCollectionAmount_is_quantity_times_rate()
        {
            Assert.Equal(200m, SettlementCalculator.ComputeCollectionAmount(5m, 40m));
        }
    }
}
