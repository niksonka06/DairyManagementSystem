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

        public async Task<IActionResult> Index(DateTime? from, DateTime? to, CancellationToken ct)
        {
            var farmer = await CurrentFarmerAsync(ct);
            if (farmer is null)
            {
                return View("~/Areas/Farmer/Views/Home/NoProfile.cshtml");
            }

            var effectiveTo = (to ?? DateTime.Today).Date;
            var effectiveFrom = (from ?? effectiveTo.AddDays(-30)).Date;

            var collections = await _collectionService.GetByFarmerAndDateRangeAsync(farmer.FarmerID, effectiveFrom, effectiveTo, ct);

            var model = collections.Select(c => new FarmerCollectionRowViewModel
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
