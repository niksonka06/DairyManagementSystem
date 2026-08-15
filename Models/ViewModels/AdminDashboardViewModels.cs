namespace DairyManagementSystem.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public DateTime OverviewDate { get; set; } = DateTime.Today;

        public decimal TotalMilkTodayLitres { get; set; }
        public decimal TotalPayableToday { get; set; }
        public int ActiveSocietyCount { get; set; }
        public int TotalSocietyCount { get; set; }
        public int ActiveFarmerCount { get; set; }
        public decimal PendingSettlementsAmount { get; set; }
        public int PendingSettlementsCount { get; set; }

        public List<SocietyPerformanceRowViewModel> Societies { get; set; } = new();
    }

    public class SocietyPerformanceRowViewModel
    {
        public int SocietyID { get; set; }
        public string SocietyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int OperatorCount { get; set; }
        public int ActiveFarmerCount { get; set; }
        public decimal TodayMilkLitres { get; set; }
        public decimal TodayPayable { get; set; }
        public int MorningEntries { get; set; }
        public int EveningEntries { get; set; }
        public decimal PendingSettlementsAmount { get; set; }
        public int PendingSettlementsCount { get; set; }
    }

    public class SocietyOverviewViewModel
    {
        public int SocietyID { get; set; }
        public string SocietyName { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public DateTime OverviewDate { get; set; } = DateTime.Today;
        public decimal TodayMilkLitres { get; set; }
        public decimal TodayPayable { get; set; }
        public int ActiveFarmerCount { get; set; }
        public int TotalFarmerCount { get; set; }
        public decimal PendingSettlementsAmount { get; set; }
        public int PendingSettlementsCount { get; set; }
        public int MorningEntries { get; set; }
        public int EveningEntries { get; set; }

        public List<SocietyOperatorRowViewModel> Operators { get; set; } = new();
        public List<string> TrendLabels { get; set; } = new();
        public List<decimal> TrendValues { get; set; } = new();
    }

    public class SocietyOperatorRowViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
