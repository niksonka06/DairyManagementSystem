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
    public class SettlementUnlockController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettlementUnlockController(IPaymentService paymentService, UserManager<ApplicationUser> userManager)
        {
            _paymentService = paymentService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? sort, string? dir, int page, CancellationToken ct)
        {
            var payments = await _paymentService.GetGeneratedAcrossAllSocietiesAsync(ct);

            var viewModel = ListPaging.Apply(
                payments.Select(p => new SettlementUnlockListItemViewModel
                {
                    PaymentID = p.PaymentID,
                    SocietyName = p.Society?.SocietyName ?? string.Empty,
                    FarmerCode = p.Farmer?.FarmerCode ?? string.Empty,
                    FarmerName = p.Farmer?.FullName ?? string.Empty,
                    PeriodStart = p.PeriodStart,
                    PeriodEnd = p.PeriodEnd,
                    NetAmount = p.NetAmount,
                    GeneratedAt = p.GeneratedAt
                }),
                sort, dir, page,
                new Dictionary<string, Func<SettlementUnlockListItemViewModel, object?>>
                {
                    ["society"] = p => p.SocietyName,
                    ["farmer"] = p => p.FarmerCode + " " + p.FarmerName,
                    ["period"] = p => p.PeriodStart,
                    ["amount"] = p => p.NetAmount,
                    ["generated"] = p => p.GeneratedAt
                },
                defaultSort: "generated",
                defaultDesc: true);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Unlock(int id, CancellationToken ct)
        {
            var payment = (await _paymentService.GetGeneratedAcrossAllSocietiesAsync(ct))
                .FirstOrDefault(p => p.PaymentID == id);
            if (payment is null)
            {
                return NotFound();
            }

            return View(new SettlementUnlockViewModel
            {
                PaymentID = id,
                RowVersion = payment.RowVersion
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(SettlementUnlockViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _paymentService.CancelGeneratedAsync(model.PaymentID, CurrentUserId(), model.Reason, model.RowVersion ?? Array.Empty<byte>(), ct);
                TempData["Success"] = "Settlement unlocked. Its collections and feed issues are editable again.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "This settlement was modified by someone else. Reload and try again.");
                return View(model);
            }
        }

        private int CurrentUserId()
        {
            var idString = _userManager.GetUserId(User)
                ?? throw new InvalidOperationException("No authenticated user id found.");
            return int.Parse(idString);
        }
    }
}
