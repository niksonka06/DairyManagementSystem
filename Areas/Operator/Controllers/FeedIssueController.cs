using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Areas.Operator.Controllers
{
    public class FeedIssueController : OperatorControllerBase
    {
        private readonly IFeedIssueService _feedIssueService;
        private readonly IFeedInventoryService _feedInventoryService;
        private readonly IFarmerService _farmerService;

        public FeedIssueController(
            IFeedIssueService feedIssueService,
            IFeedInventoryService feedInventoryService,
            IFarmerService farmerService,
            UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _feedIssueService = feedIssueService;
            _feedInventoryService = feedInventoryService;
            _farmerService = farmerService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var issues = await _feedIssueService.GetBySocietyAsync(societyId, ct);

            var viewModel = issues.Select(i => new FeedIssueListItemViewModel
            {
                IssueID = i.IssueID,
                FarmerCode = i.Farmer?.FarmerCode ?? string.Empty,
                FarmerName = i.Farmer?.FullName ?? string.Empty,
                ItemType = i.ItemType,
                FeedName = i.FeedItem?.FeedName ?? string.Empty,
                Quantity = i.Quantity,
                TotalCost = i.TotalCost,
                IssueDate = i.IssueDate
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var model = new FeedIssueFormViewModel
            {
                AvailableItems = await AvailableItemsAsync(ct),
                AvailableFarmers = await AvailableFarmersAsync(ct)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FeedIssueFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                model.AvailableItems = await AvailableItemsAsync(ct);
                model.AvailableFarmers = await AvailableFarmersAsync(ct);
                return View(model);
            }

            try
            {
                var issue = await _feedIssueService.IssueToFarmerAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"Issued {issue.Quantity} — Total cost: ₹{issue.TotalCost:0.00}";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableItems = await AvailableItemsAsync(ct);
                model.AvailableFarmers = await AvailableFarmersAsync(ct);
                return View(model);
            }
        }

        private async Task<List<FeedInventoryListItemViewModel>> AvailableItemsAsync(CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var items = await _feedInventoryService.GetBySocietyAsync(societyId, ct);
            return items.Where(f => f.IsActive).Select(f => new FeedInventoryListItemViewModel
            {
                FeedItemID = f.FeedItemID,
                ItemType = f.ItemType,
                FeedName = f.FeedName,
                Unit = f.Unit,
                PricePerUnit = f.PricePerUnit,
                StockQuantity = f.StockQuantity,
                LowStockThreshold = f.LowStockThreshold
            }).ToList();
        }

        private async Task<List<FarmerListItemViewModel>> AvailableFarmersAsync(CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var farmers = await _farmerService.GetBySocietyAsync(societyId, ct);
            return farmers.Where(f => f.IsActive).Select(f => new FarmerListItemViewModel
            {
                FarmerID = f.FarmerID,
                FarmerCode = f.FarmerCode,
                FullName = f.FullName
            }).ToList();
        }
    }
}
