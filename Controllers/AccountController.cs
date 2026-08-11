using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Deliberately identical error message whether the email doesn't
            // exist or the password is wrong — telling an attacker "that
            // email isn't registered" is a user-enumeration leak.
            const string genericError = "Invalid email or password.";

            if (user is null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, genericError);
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, genericError);
                return View(model);
            }

            if (user.MustChangePassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            return RedirectToLocalOrRoleHome(model.ReturnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            // Refresh the sign-in cookie so it reflects the updated user state.
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToLocalOrRoleHome(null);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Centralizes the "where does this user land after login" decision.
        // Never trusts an external returnUrl blindly (open-redirect protection
        // via Url.IsLocalUrl) — an attacker could otherwise craft a login link
        // with ?returnUrl=https://evil.example.com.
        private IActionResult RedirectToLocalOrRoleHome(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Role-based landing pages point at each Area's Home controller.
            // These controllers don't exist yet — they arrive with each
            // Area's own module (Stage 4 Admin/Society, Stage 7 Operator
            // collection screens, Stage 13 Farmer portal). Until then this
            // will 404 for non-Admins, which is expected at this stage.
            if (User.IsInRole(Roles.Admin)) return RedirectToAction("Index", "Home", new { area = "Admin" });
            if (User.IsInRole(Roles.Operator)) return RedirectToAction("Index", "Home", new { area = "Operator" });
            if (User.IsInRole(Roles.Farmer)) return RedirectToAction("Index", "Home", new { area = "Farmer" });

            return RedirectToAction("Index", "Home");
        }
    }
}
