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
    [Area("Operator")]
    [Authorize(Roles = Roles.Operator)]
    public class SettlementController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IFarmerService _farmerService;
        private readonly IAdvancePaymentService _advancePaymentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettlementController(
            IPaymentService paymentService,
            IFarmerService farmerService,
            IAdvancePaymentService advancePaymentService,
            UserManager<ApplicationUser> userManager)
        {
            _paymentService = paymentService;
            _farmerService = farmerService;
            _advancePaymentService = advancePaymentService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var payments = await _paymentService.GetBySocietyAsync(societyId, ct);

            var viewModel = payments.Select(p => new SettlementListItemViewModel
            {
                PaymentID = p.PaymentID,
                FarmerCode = p.Farmer?.FarmerCode ?? string.Empty,
                FarmerName = p.Farmer?.FullName ?? string.Empty,
                PeriodStart = p.PeriodStart,
                PeriodEnd = p.PeriodEnd,
                NetAmount = p.NetAmount,
                Status = p.Status
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var model = new SettlementCreateViewModel
            {
                AvailableFarmers = await AvailableFarmersAsync(ct)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SettlementCreateViewModel model, CancellationToken ct)
        {
            model.SocietyID = await CurrentOperatorSocietyIdAsync();

            if (!ModelState.IsValid)
            {
                model.AvailableFarmers = await AvailableFarmersAsync(ct);
                return View(model);
            }

            try
            {
                var payment = await _paymentService.CreateDraftAsync(model, CurrentUserId(), ct);
                TempData["Success"] = "Draft settlement created. Review and Generate when ready.";
                return RedirectToAction(nameof(Details), new { id = payment.PaymentID });
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableFarmers = await AvailableFarmersAsync(ct);
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var payment = await _paymentService.GetByIdWithinSocietyAsync(id, societyId, ct);
            if (payment is null)
            {
                return NotFound();
            }

            var model = new SettlementDetailsViewModel
            {
                PaymentID = payment.PaymentID,
                FarmerCode = payment.Farmer?.FarmerCode ?? string.Empty,
                FarmerName = payment.Farmer?.FullName ?? string.Empty,
                PeriodStart = payment.PeriodStart,
                PeriodEnd = payment.PeriodEnd,
                GrossAmount = payment.GrossAmount,
                FeedDeduction = payment.FeedDeduction,
                MedicineDeduction = payment.MedicineDeduction,
                OtherDeductionsTotal = payment.OtherDeductionsTotal,
                PreviousDue = payment.PreviousDue,
                AdvancePaid = payment.AdvancePaid,
                NetAmount = payment.NetAmount,
                Status = payment.Status,
                GeneratedAt = payment.GeneratedAt,
                PaidAt = payment.PaidAt,
                CancellationReason = payment.CancellationReason,
                DeductionLines = payment.Deductions.Select(d => (d.DeductionType, d.Amount)).ToList(),
                RowVersion = payment.RowVersion
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recalculate(int id, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            try
            {
                await _paymentService.RecalculateAsync(id, societyId, ct);
                TempData["Success"] = "Amounts recalculated from current unlocked records.";
            }
            catch (BusinessRuleException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int id, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            try
            {
                await _paymentService.GenerateAsync(id, societyId, CurrentUserId(), ct);
                TempData["Success"] = "Settlement generated. Collections and feed issues for this period are now locked.";
            }
            catch (BusinessRuleException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            try
            {
                await _paymentService.MarkPaidAsync(id, societyId, CurrentUserId(), ct);
                TempData["Success"] = "Settlement marked as Paid.";
            }
            catch (BusinessRuleException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelDraft(int id, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            try
            {
                await _paymentService.CancelDraftAsync(id, societyId, CurrentUserId(), ct);
                TempData["Success"] = "Draft settlement cancelled.";
            }
            catch (BusinessRuleException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
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
