using Microsoft.AspNetCore.Identity;

namespace DairyManagementSystem.Services
{
    // Identity's default hasher is PBKDF2. The project NFR requires BCrypt,
    // so this replaces IPasswordHasher while still accepting existing PBKDF2
    // hashes and asking Identity to rehash them on the next successful login.
    public class BCryptPasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        private const int WorkFactor = 12;
        private readonly PasswordHasher<TUser> _legacyHasher = new();

        public string HashPassword(TUser user, string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || providedPassword is null)
            {
                return PasswordVerificationResult.Failed;
            }

            if (hashedPassword.StartsWith("$2", StringComparison.Ordinal))
            {
                return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword)
                    ? PasswordVerificationResult.Success
                    : PasswordVerificationResult.Failed;
            }

            var legacy = _legacyHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return legacy is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Failed;
        }
    }
}
