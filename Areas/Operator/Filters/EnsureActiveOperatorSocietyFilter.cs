using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DairyManagementSystem.Areas.Operator.Filters
{
    // Runs before every Operator-area action. Blocks operators whose society
    // is missing or deactivated — including mid-session if an Admin deactivates
    // the society while they're still logged in.
    public class EnsureActiveOperatorSocietyFilter : IAsyncActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISocietyRepository _societyRepository;

        public EnsureActiveOperatorSocietyFilter(
            UserManager<ApplicationUser> userManager,
            ISocietyRepository societyRepository)
        {
            _userManager = userManager;
            _societyRepository = societyRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                await next();
                return;
            }

            if (context.RouteData.Values["area"]?.ToString() != "Operator")
            {
                await next();
                return;
            }

            var user = await _userManager.GetUserAsync(context.HttpContext.User);
            if (user?.SocietyID is null)
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

            await next();
        }
    }
}
