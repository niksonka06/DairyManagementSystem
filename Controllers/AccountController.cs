using System.Net;
using System.Text;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace DairyManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISocietyRepository _societyRepository;
        private readonly IFarmerRepository _farmerRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ISocietyRepository societyRepository,
            IFarmerRepository farmerRepository,
            IEmailService emailService,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _societyRepository = societyRepository;
            _farmerRepository = farmerRepository;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("LoginPolicy")]
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

            var societyBlockMessage = await GetSocietyAccessBlockMessageAsync(user);
            if (societyBlockMessage is not null)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, societyBlockMessage);
                return View(model);
            }

            if (user.MustChangePassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            return RedirectToLocalOrRoleHome(model.ReturnUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Same generic confirmation whether or not the account exists —
            // revealing "that email isn't registered" is a user-enumeration leak.
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                _logger.LogWarning("Forgot password: no account for {Email}. No email sent.", model.Email);
            }
            else if (!user.IsActive)
            {
                _logger.LogWarning("Forgot password: account {Email} is inactive. No email sent.", model.Email);
            }
            else if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning("Forgot password: account {UserId} has no email. No email sent.", user.Id);
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var callbackUrl = Url.Action(
                    action: nameof(ResetPassword),
                    controller: "Account",
                    values: new { area = "", email = user.Email, code },
                    protocol: Request.Scheme)!;

                _logger.LogWarning("Forgot password: sending reset mail to {Email}.", user.Email);
                await _emailService.SendAsync(
                    user.Email,
                    "Reset your Smart Dairy Cooperative password",
                    BuildPasswordResetEmailHtml(user.FullName, callbackUrl));
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? email, string? code)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            return View(new ResetPasswordViewModel { Email = email, Code = code });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            const string genericError = "The reset link is invalid or has expired. Request a new one.";

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, genericError);
                return View(model);
            }

            string token;
            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, genericError);
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded)
            {
                var passwordErrors = result.Errors
                    .Where(e => e.Code.StartsWith("Password", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (passwordErrors.Count > 0)
                {
                    foreach (var error in passwordErrors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, genericError);
                }

                return View(model);
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);

            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
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
        [EnableRateLimiting("LoginPolicy")]
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
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<string?> GetSocietyAccessBlockMessageAsync(ApplicationUser user)
        {
            if (await _userManager.IsInRoleAsync(user, Roles.Operator))
            {
                if (!user.SocietyID.HasValue)
                {
                    return "Your operator account is not assigned to a society. Contact an Admin.";
                }

                var society = await _societyRepository.GetByIdAsync(user.SocietyID.Value);
                if (society is null || !society.IsActive)
                {
                    return "Your society is not active. Contact the system administrator.";
                }
            }

            if (await _userManager.IsInRoleAsync(user, Roles.Farmer))
            {
                var farmer = await _farmerRepository.GetByUserIdAsync(user.Id);
                if (farmer is not null)
                {
                    var society = await _societyRepository.GetByIdAsync(farmer.SocietyID);
                    if (society is null || !society.IsActive)
                    {
                        return "Your society is not active. Contact your society operator.";
                    }
                }
            }

            return null;
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

            if (User.IsInRole(Roles.Admin)) return RedirectToAction("Index", "Home", new { area = "Admin" });
            if (User.IsInRole(Roles.Operator)) return RedirectToAction("Index", "Home", new { area = "Operator" });
            if (User.IsInRole(Roles.Farmer)) return RedirectToAction("Index", "Home", new { area = "Farmer" });

            return RedirectToAction("Index", "Home");
        }

        private static string BuildPasswordResetEmailHtml(string fullName, string resetUrl)
        {
            var name = string.IsNullOrWhiteSpace(fullName) ? "there" : WebUtility.HtmlEncode(fullName);
            var url = WebUtility.HtmlEncode(resetUrl);

            return $"""
                <p>Hello {name},</p>
                <p>We received a request to reset your Smart Dairy Cooperative password. This link expires in 2 hours.</p>
                <p><a href="{url}">Reset your password</a></p>
                <p>If you did not request this, you can ignore this email. Your password will not change.</p>
                """;
        }
    }
}
