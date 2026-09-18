using DairyManagementSystem.Helpers;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Tests
{
    public class CollectionQualityTests
    {
        [Fact]
        public void Rejected_collection_pays_zero()
        {
            Assert.Equal(0m, SettlementCalculator.ComputeCollectionAmount(12.5m, 0m));
        }

        [Fact]
        public void Receipt_pdf_includes_rejected_quality_status()
        {
            var collection = new MilkCollection
            {
                CollectionID = 7,
                CollectionDate = new DateTime(2026, 9, 18),
                Shift = Shift.Evening,
                Quantity = 8m,
                FatPercent = 3.2m,
                SNF = 8.1m,
                CLR = 27m,
                RatePerLitre = 0m,
                Amount = 0m,
                IsRejected = true,
                RejectionReason = "Sour / high acidity"
            };

            var pdf = CollectionReceiptPdf.Generate(collection, "F002", "Test Farmer", "Demo Society");
            Assert.True(pdf.Length > 100);
            Assert.Equal('%', (char)pdf[0]);
        }
    }
}
