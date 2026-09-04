namespace DairyManagementSystem.Helpers
{
    public readonly record struct SettlementTotals(
        decimal NetAmount,
        decimal OpeningBalance,
        decimal ClosingBalance);

    public static class SettlementCalculator
    {
        // Synopsis formula:
        // Gross - Feed - Medicine - Other + PreviousDue - AdvancePaid = NetAmount
        //
        // OpeningBalance is the unpaid carry-in (PreviousDue).
        // ClosingBalance is the result of this cycle (NetAmount) — what remains
        // payable or to carry into the next week.
        public static SettlementTotals Compute(
            decimal gross,
            decimal feedDeduction,
            decimal medicineDeduction,
            decimal otherDeductionsTotal,
            decimal previousDue,
            decimal advancePaid)
        {
            var net = gross - feedDeduction - medicineDeduction - otherDeductionsTotal + previousDue - advancePaid;
            return new SettlementTotals(net, previousDue, net);
        }

        public static decimal ComputeCollectionAmount(decimal quantity, decimal ratePerLitre)
        {
            return quantity * ratePerLitre;
        }
    }
}
