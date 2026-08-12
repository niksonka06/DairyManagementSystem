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
    public class FarmerController : Controller
    {
        private readonly IFarmerService _farmerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FarmerController(IFarmerService farmerService, UserManager<ApplicationUser> userManager)
        {
            _farmerService = farmerService;
            _userManager = userManager;
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
        public IActionResult Create()
        {
            return View(new FarmerFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FarmerFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync(); // never trust a posted SocietyID

            if (!ModelState.IsValid)
            {
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
                ModelState.AddModelError(nameof(model.FarmerCode), ex.Message);
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
                ModelState.AddModelError(string.Empty, ex.Message);
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
        public async Task<IActionResult> ToggleActive(int id, bool activate, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();

            try
            {
                await _farmerService.SetActiveStatusAsync(id, societyId, activate, CurrentUserId(), ct);
                TempData["Success"] = activate ? "Farmer activated." : "Farmer deactivated.";
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

        // Always derives the Operator's own society from their authenticated
        // identity — an Operator can NEVER pass a different SocietyID in the
        // URL or a hidden form field to reach another society's farmers.
        // This is the server-side enforcement the synopsis requires; nothing
        // here depends on the UI hiding a field.
        private async Task<int> CurrentOperatorSocietyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException("No authenticated user found.");

            return user.SocietyID
                ?? throw new InvalidOperationException("This Operator account has no SocietyID assigned. Contact an Admin.");
        }
    }
}
