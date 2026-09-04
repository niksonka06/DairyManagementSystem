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
    public class RatesController : Controller
    {
        private readonly IFarmerService _farmerService;
        private readonly IMilkRateService _milkRateService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatesController(IFarmerService farmerService, IMilkRateService milkRateService, UserManager<ApplicationUser> userManager)
        {
            _farmerService = farmerService;
            _milkRateService = milkRateService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? sort, string? dir, int page, CancellationToken ct)
        {
            var farmer = await CurrentFarmerAsync(ct);
            if (farmer is null)
            {
                return View("~/Areas/Farmer/Views/Home/NoProfile.cshtml");
            }

            var rates = await _milkRateService.GetBySocietyAsync(farmer.SocietyID, ct);

            var model = ListPaging.Apply(
                rates.Select(r => new MilkRateListItemViewModel
                {
                    RateID = r.RateID,
                    FatPercentFrom = r.FatPercentFrom,
                    FatPercentTo = r.FatPercentTo,
                    SnfPercentFrom = r.SnfPercentFrom,
                    SnfPercentTo = r.SnfPercentTo,
                    ClrFrom = r.ClrFrom,
                    ClrTo = r.ClrTo,
                    RatePerLitre = r.RatePerLitre,
                    EffectiveFrom = r.EffectiveFrom,
                    IsActive = r.IsActive
                }),
                sort, dir, page,
                new Dictionary<string, Func<MilkRateListItemViewModel, object?>>
                {
                    ["fat"] = r => r.FatPercentFrom,
                    ["snf"] = r => r.SnfPercentFrom,
                    ["clr"] = r => r.ClrFrom,
                    ["rate"] = r => r.RatePerLitre,
                    ["effective"] = r => r.EffectiveFrom,
                    ["status"] = r => r.IsActive
                },
                defaultSort: "effective",
                defaultDesc: true,
                activeFirst: r => r.IsActive);

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
