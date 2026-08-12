using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Services
{
    // No repository here, by design — ApplicationUser is Identity's own
    // entity, and UserManager<T> already IS Identity's service/data-access
    // layer (same reasoning as AccountController and DbInitializer in
    // Stage 3). Wrapping UserManager in another repository would just be a
    // pass-through with no logic of its own.
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public UserManagementService(UserManager<ApplicationUser> userManager, IAuditService auditService, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ApplicationUser>> GetOperatorsAsync(CancellationToken ct = default)
        {
            var operatorRoleUsers = await _userManager.GetUsersInRoleAsync(Roles.Operator);
            return operatorRoleUsers.OrderBy(u => u.FullName).ToList();
        }

        public async Task<OperatorCredentialsViewModel> CreateOperatorAsync(OperatorFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing is not null)
            {
                throw new BusinessRuleException($"Email '{model.Email}' is already registered.");
            }

            var temporaryPassword = TemporaryPasswordGenerator.Generate();

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FullName = model.FullName.Trim(),
                SocietyID = model.SocietyID,
                IsActive = true,
                MustChangePassword = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, temporaryPassword);
            if (!result.Succeeded)
            {
                throw new BusinessRuleException(
                    "Could not create the Operator account: " + string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(user, Roles.Operator);

            _auditService.Log(nameof(ApplicationUser), user.Id, AuditAction.Created,
                oldValue: null,
                newValue: new { user.FullName, user.Email, user.SocietyID, Role = Roles.Operator },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct); // persists the audit row (UserManager.CreateAsync already committed the user itself)

            return new OperatorCredentialsViewModel
            {
                FullName = user.FullName,
                LoginEmail = user.Email,
                TemporaryPassword = temporaryPassword
            };
        }

        public async Task SetActiveStatusAsync(int userId, bool isActive, int performedByUserId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new BusinessRuleException("User not found.");

            if (user.IsActive == isActive)
            {
                return;
            }

            user.IsActive = isActive;
            await _userManager.UpdateAsync(user);

            _auditService.Log(nameof(ApplicationUser), user.Id,
                isActive ? AuditAction.Activated : AuditAction.Deactivated,
                oldValue: new { IsActive = !isActive },
                newValue: new { IsActive = isActive },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
