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
    public class FarmerController : OperatorControllerBase
    {
        private readonly IFarmerService _farmerService;

        public FarmerController(IFarmerService farmerService, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _farmerService = farmerService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var farmers = await _farmerService.GetBySocietyAsync(societyId, ct);

            var viewModel = farmers.Select(f => new FarmerListItemViewModel
            {
                FarmerID = f.FarmerID,
                FarmerCode = f.FarmerCode,
                FullName = f.FullName,
                Phone = f.Phone,
                IsActive = f.IsActive
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            return View(new FarmerFormViewModel
            {
                FarmerCode = await _farmerService.GetNextFarmerCodeAsync(societyId, ct)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FarmerFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync(); // never trust a posted SocietyID

            if (!ModelState.IsValid)
            {
                model.FarmerCode = await _farmerService.GetNextFarmerCodeAsync(model.SocietyID, ct);
                return View(model);
            }

            try
            {
                var credentials = await _farmerService.CreateAsync(model, CurrentUserId(), ct);
                // Shown exactly once — see FarmerCredentialsViewModel for why
                // this can't be retrieved again later.
                return View("Credentials", credentials);
            }
            catch (BusinessRuleException ex)
            {
                if (ex.Message.Contains("Email", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.Email), ex.Message);
                }
                else if (ex.Message.Contains("Farmer code", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.FarmerCode), ex.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                model.FarmerCode = await _farmerService.GetNextFarmerCodeAsync(model.SocietyID, ct);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var farmer = await _farmerService.GetByIdWithinSocietyAsync(id, societyId, ct);
            if (farmer is null)
            {
                return NotFound();
            }

            var model = new FarmerFormViewModel
            {
                FarmerID = farmer.FarmerID,
                FarmerCode = farmer.FarmerCode,
                FullName = farmer.FullName,
                Email = farmer.User?.Email ?? string.Empty,
                Phone = farmer.Phone,
                Address = farmer.Address,
                BankAccountNo = farmer.BankAccountNo,
                BankName = farmer.BankName,
                IFSC = farmer.IFSC,
                SocietyID = farmer.SocietyID,
                RowVersion = farmer.RowVersion
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FarmerFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync(); // re-derive, never trust the posted value

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _farmerService.UpdateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"Farmer '{model.FullName}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                if (ex.Message.Contains("Email", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.Email), ex.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                return View(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty,
                    "This farmer's record was modified by someone else while you were editing it. Please reload and try again.");
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
                await _farmerService.SetActiveStatusAsync(id, societyId, shouldActivate, CurrentUserId(), ct);
                TempData["Success"] = shouldActivate ? "Farmer activated." : "Farmer deactivated.";
            }
            catch (BusinessRuleException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
