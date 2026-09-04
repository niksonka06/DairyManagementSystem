using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Areas.Operator.Controllers
{
    public class FeedInventoryController : OperatorControllerBase
    {
        private readonly IFeedInventoryService _feedInventoryService;

        public FeedInventoryController(IFeedInventoryService feedInventoryService, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _feedInventoryService = feedInventoryService;
        }

        public async Task<IActionResult> Index(string? sort, string? dir, int page, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var items = await _feedInventoryService.GetBySocietyAsync(societyId, ct);

            var all = items.Select(f => new FeedInventoryListItemViewModel
            {
                FeedItemID = f.FeedItemID,
                ItemType = f.ItemType,
                FeedName = f.FeedName,
                Unit = f.Unit,
                PricePerUnit = f.PricePerUnit,
                StockQuantity = f.StockQuantity,
                LowStockThreshold = f.LowStockThreshold,
                IsActive = f.IsActive
            }).ToList();

            ViewBag.LowStockCount = all.Count(m => m.IsLowStock && m.IsActive);

            var viewModel = ListPaging.Apply(
                all, sort, dir, page,
                new Dictionary<string, Func<FeedInventoryListItemViewModel, object?>>
                {
                    ["type"] = f => f.ItemType.ToString(),
                    ["name"] = f => f.FeedName,
                    ["unit"] = f => f.Unit,
                    ["price"] = f => f.PricePerUnit,
                    ["stock"] = f => f.StockQuantity,
                    ["status"] = f => f.IsActive
                },
                defaultSort: "name",
                activeFirst: f => f.IsActive);

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new FeedInventoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FeedInventoryFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _feedInventoryService.CreateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"'{model.FeedName}' added. Use 'Add Stock' to record initial stock.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(nameof(model.FeedName), ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var items = await _feedInventoryService.GetBySocietyAsync(societyId, ct);
            var item = items.FirstOrDefault(f => f.FeedItemID == id);
            if (item is null)
            {
                return NotFound();
            }

            var model = new FeedInventoryFormViewModel
            {
                FeedItemID = item.FeedItemID,
                ItemType = item.ItemType,
                FeedName = item.FeedName,
                Unit = item.Unit,
                PricePerUnit = item.PricePerUnit,
                LowStockThreshold = item.LowStockThreshold,
                SocietyID = item.SocietyID,
                RowVersion = item.RowVersion
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FeedInventoryFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _feedInventoryService.UpdateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = "Item updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty,
                    "This item was modified by someone else while you were editing it. Please reload and try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddStock(int id, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var items = await _feedInventoryService.GetBySocietyAsync(societyId, ct);
            var item = items.FirstOrDefault(f => f.FeedItemID == id);
            if (item is null)
            {
                return NotFound();
            }

            return View(new AddStockViewModel
            {
                FeedItemID = item.FeedItemID,
                FeedName = item.FeedName,
                SocietyID = item.SocietyID
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock(AddStockViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _feedInventoryService.AddStockAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"Added {model.QuantityToAdd} to stock.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, string activate, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var shouldActivate = FormBindingHelpers.ParseBoolFormValue(activate);

            try
            {
                await _feedInventoryService.SetActiveStatusAsync(id, societyId, shouldActivate, CurrentUserId(), ct);
                TempData["Success"] = shouldActivate ? "Item activated." : "Item deactivated.";
            }
            catch (BusinessRuleException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
