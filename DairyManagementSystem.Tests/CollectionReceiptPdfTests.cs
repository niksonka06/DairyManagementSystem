using DairyManagementSystem.Helpers;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Tests
{
    public class CollectionReceiptPdfTests
    {
        [Fact]
        public void Generate_returns_a_pdf_for_a_collection_entry()
        {
            var collection = new MilkCollection
            {
                CollectionID = 42,
                CollectionDate = new DateTime(2026, 9, 1),
                Shift = Shift.Morning,
                Quantity = 12.5m,
                FatPercent = 4.2m,
                SNF = 8.5m,
                CLR = 28m,
                RatePerLitre = 41m,
                Amount = 512.5m
            };

            var pdf = CollectionReceiptPdf.Generate(collection, "F001", "Test Farmer", "Demo Society");

            Assert.True(pdf.Length > 100);
            Assert.Equal('%', (char)pdf[0]);
            Assert.Equal('P', (char)pdf[1]);
            Assert.Equal('D', (char)pdf[2]);
            Assert.Equal('F', (char)pdf[3]);
        }
    }
}
