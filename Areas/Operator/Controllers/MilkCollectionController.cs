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
    public class MilkCollectionController : OperatorControllerBase
    {
        private readonly IMilkCollectionService _collectionService;
        private readonly IFarmerService _farmerService;

        public MilkCollectionController(IMilkCollectionService collectionService, IFarmerService farmerService, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _collectionService = collectionService;
            _farmerService = farmerService;
        }

        public async Task<IActionResult> Index(DateTime? date, Shift? shift, string? sort, string? dir, int page, CancellationToken ct)
        {
            var day = (date ?? DateTime.Today).Date;
            var form = new MilkCollectionFormViewModel
            {
                CollectionDate = day,
                Shift = shift ?? Shift.Morning
            };

            return View(await BuildPageAsync(day, form, sort, dir, page, ct));
        }

        [HttpGet]
        public IActionResult Create(DateTime? date)
        {
            return RedirectToAction(nameof(Index), new { date });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(Prefix = "Form")] MilkCollectionFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();
            var day = model.CollectionDate.Date;

            if (!ModelState.IsValid)
            {
                return View(nameof(Index), await BuildPageAsync(day, model, null, null, 1, ct));
            }

            try
            {
                var collection = await _collectionService.CreateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"Collection recorded — Amount: ₹{collection.Amount:0.00}";
                return RedirectToAction(nameof(Index), new { date = collection.CollectionDate, shift = collection.Shift });
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(nameof(Index), await BuildPageAsync(day, model, null, null, 1, ct));
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

        private async Task<MilkCollectionPageViewModel> BuildPageAsync(
            DateTime day,
            MilkCollectionFormViewModel form,
            string? sort,
            string? dir,
            int page,
            CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var collections = await _collectionService.GetBySocietyAndDateAsync(societyId, day, ct);

            var items = collections.Select(c => new MilkCollectionListItemViewModel
            {
                CollectionID = c.CollectionID,
                FarmerCode = c.Farmer?.FarmerCode ?? string.Empty,
                FarmerName = c.Farmer?.FullName ?? string.Empty,
                CollectionDate = c.CollectionDate,
                Shift = c.Shift,
                Quantity = c.Quantity,
                FatPercent = c.FatPercent,
                SNF = c.SNF,
                CLR = c.CLR,
                RatePerLitre = c.RatePerLitre,
                Amount = c.Amount,
                IsLocked = c.IsLocked
            }).ToList();

            form.CollectionDate = form.CollectionDate == default ? day : form.CollectionDate.Date;
            form.AvailableFarmers = await AvailableFarmersAsync(ct);

            var dateValue = day.ToString("yyyy-MM-dd");
            var extra = new Dictionary<string, string?> { ["date"] = dateValue };
            if (form.Shift != default)
            {
                extra["shift"] = form.Shift.ToString();
            }

            return new MilkCollectionPageViewModel
            {
                SelectedDate = day,
                Form = form,
                Collections = ListPaging.Apply(
                    items, sort, dir, page,
                    new Dictionary<string, Func<MilkCollectionListItemViewModel, object?>>
                    {
                        ["shift"] = c => c.Shift.ToString(),
                        ["farmer"] = c => c.FarmerCode + " " + c.FarmerName,
                        ["qty"] = c => c.Quantity,
                        ["fat"] = c => c.FatPercent,
                        ["snf"] = c => c.SNF,
                        ["clr"] = c => c.CLR,
                        ["rate"] = c => c.RatePerLitre,
                        ["amount"] = c => c.Amount,
                        ["status"] = c => c.IsLocked
                    },
                    defaultSort: "farmer",
                    extraRoute: extra),
                DayTotalQuantity = items.Sum(v => v.Quantity),
                DayTotalAmount = items.Sum(v => v.Amount)
            };
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
