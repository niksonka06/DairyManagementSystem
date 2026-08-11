using System.ComponentModel.DataAnnotations;

namespace DairyManagementSystem.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        // Where to send the user back to after login, if they were
        // redirected here from a protected page. Never trust this blindly —
        // see the Url.IsLocalUrl check in AccountController.
        public string? ReturnUrl { get; set; }
    }
}
