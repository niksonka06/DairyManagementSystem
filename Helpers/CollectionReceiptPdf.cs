using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Helpers
{
    public static class CollectionReceiptPdf
    {
        public static byte[] Generate(MilkCollection collection, string farmerCode, string farmerName, string societyName)
        {
            var sections = new List<PdfSection>
            {
                new PdfSection
                {
                    Title = $"{farmerCode} — {farmerName}",
                    Headers = new[] { "Particulars", "Value" },
                    Rows = new List<string[]>
                    {
                        new[] { "Society", societyName },
                        new[] { "Date", collection.CollectionDate.ToString("dd-MMM-yyyy") },
                        new[] { "Shift", collection.Shift.ToString() },
                        new[] { "Quantity (L)", collection.Quantity.ToString("0.00") },
                        new[] { "Fat %", collection.FatPercent.ToString("0.00") },
                        new[] { "SNF", collection.SNF?.ToString("0.00") ?? "—" },
                        new[] { "CLR", collection.CLR?.ToString("0.00") ?? "—" },
                        new[] { "Rate / litre", collection.IsRejected ? "—" : $"Rs.{collection.RatePerLitre:0.00}" },
                        new[] { "Amount", collection.IsRejected ? "Rs.0.00 (rejected)" : $"Rs.{collection.Amount:0.00}" },
                        new[] { "Quality", collection.IsRejected ? $"Rejected — {collection.RejectionReason}" : "Accepted" }
                    }
                }
            };

            var summary = collection.IsRejected
                ? new[] { "This entry was rejected for quality. Amount payable: Rs.0.00" }
                : new[] { $"Amount payable for this entry: Rs.{collection.Amount:0.00}" };

            return PdfReportGenerator.Generate(
                "Milk Collection Receipt",
                $"Receipt #{collection.CollectionID}  |  {collection.CollectionDate:dd-MMM-yyyy} {collection.Shift}",
                sections,
                summary);
        }
    }
}
