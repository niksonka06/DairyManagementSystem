using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DairyManagementSystem.Areas.Operator.Filters
{
    public class EnsureActiveOperatorSocietyFilter : IAsyncActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISocietyRepository _societyRepository;
        private readonly IFarmerRepository _farmerRepository;

        public EnsureActiveOperatorSocietyFilter(
            UserManager<ApplicationUser> userManager,
            ISocietyRepository societyRepository,
            IFarmerRepository farmerRepository)
        {
            _userManager = userManager;
            _societyRepository = societyRepository;
            _farmerRepository = farmerRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            var area = context.RouteData.Values["area"]?.ToString();
            var controller = context.RouteData.Values["controller"]?.ToString();
            var isOperatorArea = string.Equals(area, "Operator", StringComparison.OrdinalIgnoreCase);
            var isFarmerArea = string.Equals(area, "Farmer", StringComparison.OrdinalIgnoreCase);
            var isReports = string.Equals(controller, "Reports", StringComparison.OrdinalIgnoreCase)
                            && string.IsNullOrEmpty(area);

            if (!isOperatorArea && !isFarmerArea && !isReports)
            {
                await next();
                return;
            }

            var user = await _userManager.GetUserAsync(context.HttpContext.User);
            if (user is null)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
                return;
            }

            if (isOperatorArea || (isReports && await _userManager.IsInRoleAsync(user, Roles.Operator)))
            {
                if (user.SocietyID is null)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
                    return;
                }

                var society = await _societyRepository.GetByIdAsync(user.SocietyID.Value);
                if (society is null || !society.IsActive)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
                    return;
                }

                context.HttpContext.Items["OperatorSocietyName"] = society.SocietyName;
                await next();
                return;
            }

            if (isFarmerArea)
            {
                var farmer = await _farmerRepository.GetByUserIdAsync(user.Id);
                if (farmer is not null)
                {
                    var society = await _societyRepository.GetByIdAsync(farmer.SocietyID);
                    if (society is null || !society.IsActive)
                    {
                        context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
                        return;
                    }
                }
            }

            await next();
        }
    }
}
