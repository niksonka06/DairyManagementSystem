using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogViewService _auditLogViewService;

        // Known entity type names, matching exactly what IAuditService.Log
        // calls use across every module (nameof(Society), nameof(Farmer),
        // etc.) — kept here as the one place this list needs updating if a
        // new auditable entity is added.
        private static readonly string[] EntityTypes =
        {
            "Society", "Farmer", "ApplicationUser", "MilkRate", "MilkCollection",
            "FeedInventory", "FeedIssue", "Payment", "AdvancePayment", "Dispatch"
        };

        public AuditLogController(IAuditLogViewService auditLogViewService)
        {
            _auditLogViewService = auditLogViewService;
        }

        public async Task<IActionResult> Index(AuditLogSearchViewModel model, CancellationToken ct)
        {
            var (items, totalCount) = await _auditLogViewService.SearchAsync(
                model.EntityType, model.EntityID, performedBy: null, model.FromDate, model.ToDate,
                model.Page, model.PageSize, ct);

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

            return View(model);
        }
    }
}
