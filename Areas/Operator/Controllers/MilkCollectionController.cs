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
    [Area("Operator")]
    [Authorize(Roles = Roles.Operator)]
    public class MilkCollectionController : Controller
    {
        private readonly IMilkCollectionService _collectionService;
        private readonly IFarmerService _farmerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MilkCollectionController(IMilkCollectionService collectionService, IFarmerService farmerService, UserManager<ApplicationUser> userManager)
        {
            _collectionService = collectionService;
            _farmerService = farmerService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(DateTime? date, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var day = (date ?? DateTime.Today).Date;

            var collections = await _collectionService.GetBySocietyAndDateAsync(societyId, day, ct);

            var viewModel = collections.Select(c => new MilkCollectionListItemViewModel
            {
                CollectionID = c.CollectionID,
                FarmerCode = c.Farmer?.FarmerCode ?? string.Empty,
                FarmerName = c.Farmer?.FullName ?? string.Empty,
                CollectionDate = c.CollectionDate,
                Shift = c.Shift,
                Quantity = c.Quantity,
                FatPercent = c.FatPercent,
                SNF = c.SNF,
                RatePerLitre = c.RatePerLitre,
                Amount = c.Amount,
                IsLocked = c.IsLocked
            }).ToList();

            ViewBag.SelectedDate = day;
            ViewBag.DayTotalQuantity = viewModel.Sum(v => v.Quantity);
            ViewBag.DayTotalAmount = viewModel.Sum(v => v.Amount);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var model = new MilkCollectionFormViewModel
            {
                AvailableFarmers = await AvailableFarmersAsync(ct)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MilkCollectionFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                model.AvailableFarmers = await AvailableFarmersAsync(ct);
                return View(model);
            }

            try
            {
                var collection = await _collectionService.CreateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"Collection recorded — Amount: ₹{collection.Amount:0.00}";
                return RedirectToAction(nameof(Index), new { date = collection.CollectionDate });
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableFarmers = await AvailableFarmersAsync(ct);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var collection = await _collectionService.GetByIdWithinSocietyAsync(id, societyId, ct);
            if (collection is null)
            {
                return NotFound();
            }

            var model = new MilkCollectionFormViewModel
            {
                CollectionID = collection.CollectionID,
                FarmerID = collection.FarmerID,
                CollectionDate = collection.CollectionDate,
                Shift = collection.Shift,
                Quantity = collection.Quantity,
                FatPercent = collection.FatPercent,
                SNF = collection.SNF,
                CLR = collection.CLR,
                SocietyID = collection.SocietyID,
                RowVersion = collection.RowVersion,
                AvailableFarmers = await AvailableFarmersAsync(ct)
            };

            ViewBag.IsLocked = collection.IsLocked;
            ViewBag.FarmerName = collection.Farmer?.FullName;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MilkCollectionFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                model.AvailableFarmers = await AvailableFarmersAsync(ct);
                return View(model);
            }

            try
            {
                await _collectionService.UpdateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = "Collection updated successfully.";
                return RedirectToAction(nameof(Index), new { date = model.CollectionDate });
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableFarmers = await AvailableFarmersAsync(ct);
                return View(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty,
                    "This collection was modified by someone else while you were editing it. Please reload and try again.");
                model.AvailableFarmers = await AvailableFarmersAsync(ct);
                return View(model);
            }
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

        private int CurrentUserId()
        {
            var idString = _userManager.GetUserId(User)
                ?? throw new InvalidOperationException("No authenticated user id found.");
            return int.Parse(idString);
        }

        private async Task<int> CurrentOperatorSocietyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException("No authenticated user found.");

            return user.SocietyID
                ?? throw new InvalidOperationException("This Operator account has no SocietyID assigned. Contact an Admin.");
        }
    }
}
