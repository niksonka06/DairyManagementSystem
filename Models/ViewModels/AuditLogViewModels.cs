using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class AuditLogListItemViewModel
    {
        public long LogID { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityID { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string PerformedByName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class AuditLogSearchViewModel
    {
        [Display(Name = "Entity Type")]
        public string? EntityType { get; set; }

        [Display(Name = "Entity ID")]
        public int? EntityID { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        public List<AuditLogListItemViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        // Populated from the AuditAction enum so the filter dropdown always
        // matches whatever action types actually exist in code — no
        // hardcoded list to fall out of sync when a new action is added.
        public List<string> AvailableEntityTypes { get; set; } = new();
    }
}
