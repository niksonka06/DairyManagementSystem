using DairyManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DairyManagementSystem.Filters
{
    public class MustChangePasswordFilter : IAsyncActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public MustChangePasswordFilter(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            {
                await next();
                return;
            }

            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            if (string.Equals(controller, "Account", StringComparison.OrdinalIgnoreCase)
                && string.Equals(action, "ChangePassword", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var user = await _userManager.GetUserAsync(context.HttpContext.User);
            if (user?.MustChangePassword == true)
            {
                context.Result = new RedirectToActionResult("ChangePassword", "Account", new { area = "" });
                return;
            }

            await next();
        }
    }
}
