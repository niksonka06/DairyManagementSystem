using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Areas.Operator.Controllers
{
    public class AdvancePaymentController : OperatorControllerBase
    {
        private readonly IAdvancePaymentService _advancePaymentService;
        private readonly IFarmerService _farmerService;

        public AdvancePaymentController(IAdvancePaymentService advancePaymentService, IFarmerService farmerService, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _advancePaymentService = advancePaymentService;
            _farmerService = farmerService;
        }

        public async Task<IActionResult> Index(string? sort, string? dir, int page, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var advances = await _advancePaymentService.GetBySocietyAsync(societyId, ct);

            var viewModel = ListPaging.Apply(
                advances.Select(a => new AdvancePaymentListItemViewModel
                {
                    AdvancePaymentID = a.AdvancePaymentID,
                    FarmerCode = a.Farmer?.FarmerCode ?? string.Empty,
                    FarmerName = a.Farmer?.FullName ?? string.Empty,
                    Amount = a.Amount,
                    PaymentDate = a.PaymentDate,
                    IsApplied = a.IsApplied
                }),
                sort, dir, page,
                new Dictionary<string, Func<AdvancePaymentListItemViewModel, object?>>
                {
                    ["date"] = a => a.PaymentDate,
                    ["farmer"] = a => a.FarmerCode + " " + a.FarmerName,
                    ["amount"] = a => a.Amount,
                    ["status"] = a => a.IsApplied
                },
                defaultSort: "date",
                defaultDesc: true);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var model = new AdvancePaymentFormViewModel
            {
                AvailableFarmers = await AvailableFarmersAsync(ct)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdvancePaymentFormViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                model.AvailableFarmers = await AvailableFarmersAsync(ct);
                return View(model);
            }

            try
            {
                await _advancePaymentService.RecordAsync(model, CurrentUserId(), ct);
                TempData["Success"] = $"Advance of ₹{model.Amount:0.00} recorded. It will apply to this farmer's next settlement.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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
    }
}
