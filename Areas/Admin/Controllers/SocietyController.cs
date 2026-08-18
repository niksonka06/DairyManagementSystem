using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class SocietyController : Controller
    {
        private readonly ISocietyService _societyService;
        private readonly IUserManagementService _userManagementService;
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SocietyController(
            ISocietyService societyService,
            IUserManagementService userManagementService,
            IAdminDashboardService adminDashboardService,
            UserManager<ApplicationUser> userManager)
        {
            _societyService = societyService;
            _userManagementService = userManagementService;
            _adminDashboardService = adminDashboardService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var societies = await _societyService.GetAllAsync(ct);

            var viewModel = societies.Select(s => new SocietyListItemViewModel
            {
                SocietyID = s.SocietyID,
                SocietyName = s.SocietyName,
                RegistrationNo = s.RegistrationNo,
                ContactPhone = s.ContactPhone,
                IsActive = s.IsActive
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, DateTime? date, CancellationToken ct)
        {
            var model = await _adminDashboardService.GetSocietyOverviewAsync(id, date, ct);
            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Setup(CancellationToken ct)
        {
            return View(new SocietySetupViewModel
            {
                RegistrationNo = await _societyService.GetNextRegistrationNoAsync(ct)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(SocietySetupViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                model.RegistrationNo = await _societyService.GetNextRegistrationNoAsync(ct);
                return View(model);
            }

            if (model.CreateOperator)
            {
                var email = model.OperatorEmail!.Trim();
                var existingOperator = await _userManager.FindByEmailAsync(email);
                if (existingOperator is not null)
                {
                    ModelState.AddModelError(nameof(model.OperatorEmail), $"Email '{email}' is already registered.");
                    model.RegistrationNo = await _societyService.GetNextRegistrationNoAsync(ct);
                    return View(model);
                }
            }

            try
            {
                var society = await _societyService.CreateAsync(model.ToSocietyForm(), CurrentUserId(), ct);

                if (!model.CreateOperator)
                {
                    TempData["Success"] = $"Society '{society.SocietyName}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                var operatorModel = new OperatorFormViewModel
                {
                    FullName = model.OperatorFullName!.Trim(),
                    Email = model.OperatorEmail!.Trim(),
                    SocietyID = society.SocietyID
                };

                var credentials = await _userManagementService.CreateOperatorAsync(operatorModel, CurrentUserId(), ct);

                return View("SetupResult", new SocietySetupResultViewModel
                {
                    SocietyName = society.SocietyName,
                    OperatorCredentials = credentials
                });
            }
            catch (BusinessRuleException ex)
            {
                if (ex.Message.Contains("Registration number", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.RegistrationNo), ex.Message);
                }
                else if (ex.Message.Contains("Email", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.OperatorEmail), ex.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                model.RegistrationNo = await _societyService.GetNextRegistrationNoAsync(ct);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return RedirectToAction(nameof(Setup));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SocietyFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _societyService.CreateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"Society '{model.SocietyName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                // Friendly message shown on the form itself, not a generic 500 —
                // e.g. "Registration number already in use."
                ModelState.AddModelError(nameof(model.RegistrationNo), ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var society = await _societyService.GetByIdAsync(id, ct);
            if (society is null)
            {
                return NotFound();
            }

            var model = new SocietyFormViewModel
            {
                SocietyID = society.SocietyID,
                SocietyName = society.SocietyName,
                RegistrationNo = society.RegistrationNo,
                Address = society.Address,
                ContactPhone = society.ContactPhone,
                RowVersion = society.RowVersion
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SocietyFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _societyService.UpdateAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"Society '{model.SocietyName}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Someone else saved a change to this exact society between
                // this user loading the Edit form and submitting it. We do
                // NOT silently overwrite their change — show a clear message
                // and make the user reload and reapply their edit.
                ModelState.AddModelError(string.Empty,
                    "This society was modified by someone else while you were editing it. Please reload the page and try again.");
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
                await _societyService.SetActiveStatusAsync(id, shouldActivate, CurrentUserId(), ct);
                TempData["Success"] = shouldActivate ? "Society activated." : "Society deactivated.";
            }
            catch (BusinessRuleException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private int CurrentUserId()
        {
            // Never trust a client-supplied user id — always derive it from
            // the authenticated identity, same rule the synopsis states
            // explicitly for Farmer data (Section 5/Farmer Portal) and which
            // applies just as much here for "who performed this admin action".
            var idString = _userManager.GetUserId(User)
                ?? throw new InvalidOperationException("No authenticated user id found.");
            return int.Parse(idString);
        }
    }
}
