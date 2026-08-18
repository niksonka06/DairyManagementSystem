using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Helpers
{
    public static class SettlementPdf
    {
        public static byte[] Generate(Payment payment, string farmerCode, string farmerName)
        {
            var rows = new List<string[]>
            {
                new[] { "Gross milk amount", $"Rs.{payment.GrossAmount:0.00}" },
                new[] { "Feed deduction", $"-Rs.{payment.FeedDeduction:0.00}" },
                new[] { "Medicine deduction", $"-Rs.{payment.MedicineDeduction:0.00}" }
            };

            foreach (var line in (payment.Deductions ?? []).OrderBy(d => d.DeductionType))
            {
                rows.Add(new[] { $"{line.DeductionType} deduction", $"-Rs.{line.Amount:0.00}" });
            }

            rows.Add(new[] { "Carry-forward from previous week", $"Rs.{payment.PreviousDue:0.00}" });
            rows.Add(new[] { "Advance paid", $"-Rs.{payment.AdvancePaid:0.00}" });
            rows.Add(new[] { "Net amount", $"Rs.{payment.NetAmount:0.00}" });

            var note = payment.NetAmount < 0
                ? "Net is negative: this amount is carried to the next weekly settlement as a deduction. No payout this week."
                : $"Status: {payment.Status}";

            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = $"{farmerCode} — {farmerName}",
                    Headers = new[] { "Particulars", "Amount" },
                    Rows = rows
                }
            };

            return PdfReportGenerator.Generate(
                "Farmer Settlement",
                $"{payment.PeriodStart:dd-MMM-yyyy} to {payment.PeriodEnd:dd-MMM-yyyy}  |  {payment.Status}",
                sections,
                new[] { note });
        }
    }
}
