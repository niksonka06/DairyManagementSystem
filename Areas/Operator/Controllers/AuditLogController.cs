using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Areas.Operator.Controllers
{
    public class AuditLogController : OperatorControllerBase
    {
        private readonly IAuditLogViewService _auditLogViewService;

        private static readonly string[] EntityTypes =
        {
            "Farmer", "MilkRate", "MilkCollection", "ShiftClose",
            "FeedInventory", "FeedIssue", "Payment", "AdvancePayment", "Dispatch"
        };

        public AuditLogController(
            IAuditLogViewService auditLogViewService,
            UserManager<ApplicationUser> userManager,
            ISocietyRepository societyRepository)
            : base(userManager, societyRepository)
        {
            _auditLogViewService = auditLogViewService;
        }

        public async Task<IActionResult> Index(AuditLogSearchViewModel model, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var (items, totalCount) = await _auditLogViewService.SearchAsync(
                model.EntityType, model.EntityID, performedBy: null, model.FromDate, model.ToDate,
                model.Page, model.PageSize, societyId, ct);

            model.Items = items.Select(a => new AuditLogListItemViewModel
            {
                LogID = a.LogID,
                EntityType = a.EntityType,
                EntityID = a.EntityID,
                Action = a.Action,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                PerformedByName = a.PerformedByUser?.FullName ?? $"User #{a.PerformedBy}",
                Timestamp = a.Timestamp
            }).ToList();

            model.TotalCount = totalCount;
            model.AvailableEntityTypes = EntityTypes.ToList();
            ViewBag.ScopeNote = "Showing audit entries for your society only (read-only).";

            return View("~/Areas/Admin/Views/AuditLog/Index.cshtml", model);
        }
    }
}
