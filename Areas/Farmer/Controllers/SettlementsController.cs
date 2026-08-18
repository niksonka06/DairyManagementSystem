using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Areas.Farmer.Controllers
{
    [Area("Farmer")]
    [Authorize(Roles = Roles.Farmer)]
    public class SettlementsController : Controller
    {
        private readonly IFarmerService _farmerService;
        private readonly IPaymentService _paymentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettlementsController(IFarmerService farmerService, IPaymentService paymentService, UserManager<ApplicationUser> userManager)
        {
            _farmerService = farmerService;
            _paymentService = paymentService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var farmer = await CurrentFarmerAsync(ct);
            if (farmer is null)
            {
                return View("~/Areas/Farmer/Views/Home/NoProfile.cshtml");
            }

            var settlements = await _paymentService.GetByFarmerAsync(farmer.FarmerID, ct);

            var model = settlements
                .Where(p => p.Status != SettlementStatus.Draft)
                .Select(p => new FarmerSettlementRowViewModel
            {
                PaymentID = p.PaymentID,
                PeriodStart = p.PeriodStart,
                PeriodEnd = p.PeriodEnd,
                NetAmount = p.NetAmount,
                Status = p.Status
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var farmer = await CurrentFarmerAsync(ct);
            if (farmer is null)
            {
                return View("~/Areas/Farmer/Views/Home/NoProfile.cshtml");
            }

            // The ownership check that matters most in this whole module:
            // GetByIdForFarmerAsync filters by BOTH paymentId AND farmerId at
            // the query level. If this settlement belongs to someone else,
            // this returns null regardless of whether `id` is a valid
            // PaymentID for a DIFFERENT farmer — there is no code path here
            // that can leak another farmer's settlement, even if they guess
            // or enumerate IDs in the URL.
            var payment = await _paymentService.GetByIdForFarmerAsync(id, farmer.FarmerID, ct);
            if (payment is null)
            {
                return NotFound();
            }

            var model = new FarmerSettlementDetailViewModel
            {
                PaymentID = payment.PaymentID,
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
                PaidAt = payment.PaidAt,
                DeductionLines = payment.Deductions.Select(d => (d.DeductionType, d.Amount)).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id, CancellationToken ct)
        {
            var farmer = await CurrentFarmerAsync(ct);
            if (farmer is null)
            {
                return NotFound();
            }

            var payment = await _paymentService.GetByIdForFarmerAsync(id, farmer.FarmerID, ct);
            if (payment is null)
            {
                return NotFound();
            }

            var pdf = SettlementPdf.Generate(payment, farmer.FarmerCode, farmer.FullName);
            return File(pdf, "application/pdf", $"Settlement_{farmer.FarmerCode}_{payment.PeriodStart:yyyyMMdd}.pdf");
        }

        private async Task<Models.Entities.Farmer?> CurrentFarmerAsync(CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return null;
            return await _farmerService.GetByUserIdAsync(user.Id, ct);
        }
    }
}
