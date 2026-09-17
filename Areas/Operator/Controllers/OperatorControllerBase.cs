using DairyManagementSystem.Areas.Operator.Filters;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Areas.Operator.Controllers
{
    [Area("Operator")]
    [Authorize(Roles = Roles.Operator)]
    [ServiceFilter(typeof(EnsureActiveOperatorSocietyFilter))]
    public abstract class OperatorControllerBase : Controller
    {
        protected UserManager<ApplicationUser> UserManager { get; }
        protected ISocietyRepository SocietyRepository { get; }

        protected OperatorControllerBase(UserManager<ApplicationUser> userManager, ISocietyRepository societyRepository)
        {
            UserManager = userManager;
            SocietyRepository = societyRepository;
        }

        protected int CurrentUserId()
        {
            var idString = UserManager.GetUserId(User)
                ?? throw new InvalidOperationException("No authenticated user id found.");
            return int.Parse(idString);
        }

        protected async Task<int> CurrentOperatorSocietyIdAsync()
        {
            var user = await UserManager.GetUserAsync(User)
                ?? throw new InvalidOperationException("No authenticated user found.");

            return user.SocietyID
                ?? throw new InvalidOperationException("This Operator account has no society assigned. Contact an Admin.");
        }

        protected async Task<string> CurrentOperatorSocietyNameAsync()
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var society = await SocietyRepository.GetByIdAsync(societyId);
            return society?.SocietyName
                ?? throw new InvalidOperationException("Society not found. Contact an Admin.");
        }
    }
}
