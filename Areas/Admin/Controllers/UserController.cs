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
            var societies = await _societyService.GetAllAsync(ct);
            var model = new OperatorFormViewModel
            {
                AvailableSocieties = societies.Select(s => new SocietyListItemViewModel
                {
                    SocietyID = s.SocietyID,
                    SocietyName = s.SocietyName
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOperator(OperatorFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var societies = await _societyService.GetAllAsync(ct);
                model.AvailableSocieties = societies.Select(s => new SocietyListItemViewModel
                {
                    SocietyID = s.SocietyID,
                    SocietyName = s.SocietyName
                }).ToList();
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
                var societies = await _societyService.GetAllAsync(ct);
                model.AvailableSocieties = societies.Select(s => new SocietyListItemViewModel
                {
                    SocietyID = s.SocietyID,
                    SocietyName = s.SocietyName
                }).ToList();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, bool activate, CancellationToken ct)
        {
            try
            {
                await _userManagementService.SetActiveStatusAsync(id, activate, CurrentUserId(), ct);
                TempData["Success"] = activate ? "Operator activated." : "Operator deactivated.";
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
    }
}
