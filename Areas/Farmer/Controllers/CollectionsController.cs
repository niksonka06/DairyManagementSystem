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
    public class CollectionsController : Controller
    {
        private readonly IFarmerService _farmerService;
        private readonly IMilkCollectionService _collectionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CollectionsController(IFarmerService farmerService, IMilkCollectionService collectionService, UserManager<ApplicationUser> userManager)
        {
            _farmerService = farmerService;
            _collectionService = collectionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? sort, string? dir, int page, CancellationToken ct)
        {
            var farmer = await CurrentFarmerAsync(ct);
            if (farmer is null)
            {
                return View("~/Areas/Farmer/Views/Home/NoProfile.cshtml");
            }

            var effectiveTo = (to ?? DateTime.Today).Date;
            var effectiveFrom = (from ?? effectiveTo.AddDays(-30)).Date;

            var collections = await _collectionService.GetByFarmerAndDateRangeAsync(farmer.FarmerID, effectiveFrom, effectiveTo, ct);

            var rows = collections.Select(c => new FarmerCollectionRowViewModel
            {
                CollectionDate = c.CollectionDate,
                Shift = c.Shift,
                Quantity = c.Quantity,
                FatPercent = c.FatPercent,
                SNF = c.SNF,
                CLR = c.CLR,
                RatePerLitre = c.RatePerLitre,
                Amount = c.Amount,
                IsLocked = c.IsLocked
            }).ToList();

            ViewBag.FromDate = effectiveFrom;
            ViewBag.ToDate = effectiveTo;
            ViewBag.TotalQuantity = rows.Sum(r => r.Quantity);
            ViewBag.TotalAmount = rows.Sum(r => r.Amount);

            var model = ListPaging.Apply(
                rows, sort, dir, page,
                new Dictionary<string, Func<FarmerCollectionRowViewModel, object?>>
                {
                    ["date"] = r => r.CollectionDate,
                    ["shift"] = r => r.Shift.ToString(),
                    ["qty"] = r => r.Quantity,
                    ["fat"] = r => r.FatPercent,
                    ["snf"] = r => r.SNF,
                    ["clr"] = r => r.CLR,
                    ["rate"] = r => r.RatePerLitre,
                    ["amount"] = r => r.Amount,
                    ["status"] = r => r.IsLocked
                },
                defaultSort: "date",
                defaultDesc: true,
                extraRoute: new Dictionary<string, string?>
                {
                    ["from"] = effectiveFrom.ToString("yyyy-MM-dd"),
                    ["to"] = effectiveTo.ToString("yyyy-MM-dd")
                });

            return View(model);
        }

        private async Task<Models.Entities.Farmer?> CurrentFarmerAsync(CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return null;
            return await _farmerService.GetByUserIdAsync(user.Id, ct);
        }
    }
}
