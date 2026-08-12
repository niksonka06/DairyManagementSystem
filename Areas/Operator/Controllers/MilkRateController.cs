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
    public class MilkRateController : Controller
    {
        private readonly IMilkRateService _milkRateService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MilkRateController(IMilkRateService milkRateService, UserManager<ApplicationUser> userManager)
        {
            _milkRateService = milkRateService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var rates = await _milkRateService.GetBySocietyAsync(societyId, ct);

            var viewModel = rates.Select(r => new MilkRateListItemViewModel
            {
                RateID = r.RateID,
                FatPercent = r.FatPercent,
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
                TempData["Success"] = $"Rate for {model.FatPercent}% fat added.";
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
                FatPercent = rate.FatPercent,
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
        public async Task<IActionResult> ToggleActive(int id, bool activate, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();

            try
            {
                await _milkRateService.SetActiveStatusAsync(id, societyId, activate, CurrentUserId(), ct);
                TempData["Success"] = activate ? "Rate activated." : "Rate deactivated.";
            }
            catch (BusinessRuleException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
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
