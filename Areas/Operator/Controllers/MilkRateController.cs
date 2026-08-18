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
    public class MilkRateController : OperatorControllerBase
    {
        private readonly IMilkRateService _milkRateService;

        public MilkRateController(IMilkRateService milkRateService, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _milkRateService = milkRateService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var rates = await _milkRateService.GetBySocietyAsync(societyId, ct);

            var viewModel = rates.Select(r => new MilkRateListItemViewModel
            {
                RateID = r.RateID,
                FatPercentFrom = r.FatPercentFrom,
                FatPercentTo = r.FatPercentTo,
                SnfPercentFrom = r.SnfPercentFrom,
                SnfPercentTo = r.SnfPercentTo,
                ClrFrom = r.ClrFrom,
                ClrTo = r.ClrTo,
                RatePerLitre = r.RatePerLitre,
                EffectiveFrom = r.EffectiveFrom,
                IsActive = r.IsActive
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new MilkRateFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MilkRateFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _milkRateService.CreateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"Rate for fat {model.FatPercentFrom:0.00}–{model.FatPercentTo:0.00}, SNF {model.SnfPercentFrom:0.00}–{model.SnfPercentTo:0.00}, CLR {model.ClrFrom:0.00}–{model.ClrTo:0.00} added.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var rates = await _milkRateService.GetBySocietyAsync(societyId, ct);
            var rate = rates.FirstOrDefault(r => r.RateID == id);
            if (rate is null)
            {
                return NotFound();
            }

            var model = new MilkRateFormViewModel
            {
                RateID = rate.RateID,
                FatPercentFrom = rate.FatPercentFrom,
                FatPercentTo = rate.FatPercentTo,
                SnfPercentFrom = rate.SnfPercentFrom,
                SnfPercentTo = rate.SnfPercentTo,
                ClrFrom = rate.ClrFrom,
                ClrTo = rate.ClrTo,
                RatePerLitre = rate.RatePerLitre,
                EffectiveFrom = rate.EffectiveFrom,
                SocietyID = rate.SocietyID,
                RowVersion = rate.RowVersion
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MilkRateFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _milkRateService.UpdateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = "Rate updated successfully.";
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
                    "This rate was modified by someone else while you were editing it. Please reload and try again.");
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
                await _milkRateService.SetActiveStatusAsync(id, societyId, shouldActivate, CurrentUserId(), ct);
                TempData["Success"] = shouldActivate ? "Rate activated." : "Rate deactivated.";
            }
            catch (BusinessRuleException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
