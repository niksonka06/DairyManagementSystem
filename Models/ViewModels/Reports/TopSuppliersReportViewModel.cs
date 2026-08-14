namespace DairyManagementSystem.Models.ViewModels.Reports
{
    public class SupplierRow
    {
        public string FarmerCode { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public decimal Value { get; set; } // quantity OR amount, depending on which list this row is in
    }

    public class TopSuppliersReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; } = DateTime.Today;

        public List<SupplierRow> TopByQuantity { get; set; } = new();
        public List<SupplierRow> TopByIncome { get; set; } = new();
    }
}
