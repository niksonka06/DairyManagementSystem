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
    public class DispatchController : OperatorControllerBase
    {
        private readonly IDispatchService _dispatchService;

        public DispatchController(IDispatchService dispatchService, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _dispatchService = dispatchService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var dispatches = await _dispatchService.GetBySocietyAsync(societyId, ct);

            var viewModel = dispatches.Select(d => new DispatchListItemViewModel
            {
                DispatchID = d.DispatchID,
                DispatchDate = d.DispatchDate,
                DispatchTime = d.DispatchTime,
                VehicleNo = d.VehicleNo,
                Destination = d.Destination,
                TotalCollected = d.TotalCollected,
                TotalDispatched = d.TotalDispatched,
                Variance = d.Variance,
                VariancePercent = d.VariancePercent,
                VarianceReason = d.VarianceReason
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var collected = await _dispatchService.GetCollectedLitresAsync(societyId, DateTime.Today, ct);

            return View(new DispatchFormViewModel
            {
                TotalCollected = collected,
                TotalDispatched = collected
            });
        }

        [HttpGet]
        public async Task<IActionResult> CollectedLitres(DateTime date, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var litres = await _dispatchService.GetCollectedLitresAsync(societyId, date, ct);
            return Json(new { litres });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DispatchFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                model.TotalCollected = await _dispatchService.GetCollectedLitresAsync(model.SocietyID, model.DispatchDate, ct);
                return View(model);
            }

            try
            {
                var dispatch = await _dispatchService.CreateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"Dispatch recorded — Variance: {dispatch.Variance:0.00}L ({dispatch.VariancePercent:0.0}%)";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.TotalCollected = await _dispatchService.GetCollectedLitresAsync(model.SocietyID, model.DispatchDate, ct);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var dispatch = await _dispatchService.GetByIdWithinSocietyAsync(id, societyId, ct);
            if (dispatch is null)
            {
                return NotFound();
            }

            var model = new DispatchFormViewModel
            {
                DispatchID = dispatch.DispatchID,
                DispatchDate = dispatch.DispatchDate,
                DispatchTime = dispatch.DispatchTime,
                VehicleNo = dispatch.VehicleNo,
                Destination = dispatch.Destination,
                TotalDispatched = dispatch.TotalDispatched,
                TotalCollected = await _dispatchService.GetCollectedLitresAsync(societyId, dispatch.DispatchDate, ct),
                VarianceReason = dispatch.VarianceReason,
                SocietyID = dispatch.SocietyID,
                RowVersion = dispatch.RowVersion
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DispatchFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                model.TotalCollected = await _dispatchService.GetCollectedLitresAsync(model.SocietyID, model.DispatchDate, ct);
                return View(model);
            }

            try
            {
                await _dispatchService.UpdateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = "Dispatch record updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.TotalCollected = await _dispatchService.GetCollectedLitresAsync(model.SocietyID, model.DispatchDate, ct);
                return View(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty,
                    "This dispatch record was modified by someone else while you were editing it. Please reload and try again.");
                model.TotalCollected = await _dispatchService.GetCollectedLitresAsync(model.SocietyID, model.DispatchDate, ct);
                return View(model);
            }
        }
    }
}
