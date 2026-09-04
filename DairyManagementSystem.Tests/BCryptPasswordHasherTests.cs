using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Services;
using Microsoft.AspNetCore.Identity;

namespace DairyManagementSystem.Tests
{
    public class BCryptPasswordHasherTests
    {
        [Fact]
        public void HashPassword_uses_bcrypt_and_verifies()
        {
            var hasher = new BCryptPasswordHasher<ApplicationUser>();
            var user = new ApplicationUser { UserName = "op@test.local" };

            var hash = hasher.HashPassword(user, "Secret1A");

            Assert.StartsWith("$2", hash);
            Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(user, hash, "Secret1A"));
            Assert.Equal(PasswordVerificationResult.Failed, hasher.VerifyHashedPassword(user, hash, "WrongPass1"));
        }
    }
}
