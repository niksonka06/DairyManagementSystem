using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class UserController : Controller
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ISocietyService _societyService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(IUserManagementService userManagementService, ISocietyService societyService, UserManager<ApplicationUser> userManager)
        {
            _userManagementService = userManagementService;
            _societyService = societyService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var operators = await _userManagementService.GetOperatorsAsync(ct);
            var societies = await _societyService.GetAllAsync(ct);
            var societyNameById = societies.ToDictionary(s => s.SocietyID, s => s.SocietyName);

            var viewModel = operators.Select(o => new OperatorListItemViewModel
            {
                UserId = o.Id,
                FullName = o.FullName,
                Email = o.Email ?? string.Empty,
                SocietyName = o.SocietyID.HasValue && societyNameById.TryGetValue(o.SocietyID.Value, out var name)
                    ? name
                    : "(unassigned)",
                IsActive = o.IsActive
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOperator(CancellationToken ct)
        {
            var model = new OperatorFormViewModel
            {
                AvailableSocieties = await LoadSocietyOptionsAsync(ct)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOperator(OperatorFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableSocieties = await LoadSocietyOptionsAsync(ct);
                return View(model);
            }

            try
            {
                var credentials = await _userManagementService.CreateOperatorAsync(model, CurrentUserId(), ct);
                return View("OperatorCredentials", credentials);
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(nameof(model.Email), ex.Message);
                model.AvailableSocieties = await LoadSocietyOptionsAsync(ct);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditOperator(int id, CancellationToken ct)
        {
            var user = await _userManagementService.GetOperatorByIdAsync(id, ct);
            if (user is null)
            {
                return NotFound();
            }

            var model = new OperatorFormViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                SocietyID = user.SocietyID ?? 0,
                AvailableSocieties = await LoadSocietyOptionsAsync(ct)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOperator(OperatorFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableSocieties = await LoadSocietyOptionsAsync(ct);
                return View(model);
            }

            try
            {
                await _userManagementService.UpdateOperatorAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"Operator '{model.FullName}' updated successfully.";
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

                model.AvailableSocieties = await LoadSocietyOptionsAsync(ct);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, string activate, CancellationToken ct)
        {
            var shouldActivate = FormBindingHelpers.ParseBoolFormValue(activate);

            try
            {
                await _userManagementService.SetActiveStatusAsync(id, shouldActivate, CurrentUserId(), ct);
                TempData["Success"] = shouldActivate ? "Operator activated." : "Operator deactivated.";
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

        private async Task<List<SocietyListItemViewModel>> LoadSocietyOptionsAsync(CancellationToken ct)
        {
            var societies = await _societyService.GetAllAsync(ct);
            return societies.Select(s => new SocietyListItemViewModel
            {
                SocietyID = s.SocietyID,
                SocietyName = s.SocietyName
            }).ToList();
        }
    }
}
