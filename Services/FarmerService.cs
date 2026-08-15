using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace DairyManagementSystem.Services
{
    public class FarmerService : IFarmerService
    {
        private readonly IFarmerRepository _farmerRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public FarmerService(
            IFarmerRepository farmerRepository,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _farmerRepository = farmerRepository;
            _userManager = userManager;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Farmer>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await _farmerRepository.GetBySocietyAsync(societyId, ct);
        }

        public async Task<Farmer?> GetByIdWithinSocietyAsync(int farmerId, int societyId, CancellationToken ct = default)
        {
            return await _farmerRepository.GetByIdWithinSocietyAsync(farmerId, societyId, ct);
        }

        public async Task<Farmer?> GetByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _farmerRepository.GetByUserIdAsync(userId, ct);
        }

        public async Task<FarmerCredentialsViewModel> CreateAsync(FarmerFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var farmerCode = model.FarmerCode.Trim();

            if (await _farmerRepository.FarmerCodeExistsInSocietyAsync(farmerCode, model.SocietyID, excludingFarmerId: null, ct))
            {
                throw new BusinessRuleException($"Farmer code '{farmerCode}' is already used in this society.");
            }

            var loginEmail = model.Email.Trim().ToLowerInvariant();
            if (await _userManager.FindByEmailAsync(loginEmail) is not null)
            {
                throw new BusinessRuleException($"Email '{loginEmail}' is already registered.");
            }

            var temporaryPassword = TemporaryPasswordGenerator.Generate();

            // Two things must succeed together: the Identity login account
            // AND the Farmer profile row. Wrapping both in one transaction —
            // UserManager and our repository share the same scoped DbContext,
            // so if either half fails, we roll back everything, never leaving
            // an orphaned login with no Farmer, or a Farmer with no login.
            await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var user = new ApplicationUser
                {
                    UserName = loginEmail,
                    Email = loginEmail,
                    FullName = model.FullName.Trim(),
                    SocietyID = model.SocietyID,
                    IsActive = true,
                    MustChangePassword = true,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, temporaryPassword);
                if (!createResult.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    throw new BusinessRuleException(
                        "Could not create the farmer's login account: " +
                        string.Join("; ", createResult.Errors.Select(e => e.Description)));
                }

                await _userManager.AddToRoleAsync(user, Roles.Farmer);

                var farmer = new Farmer
                {
                    FarmerCode = farmerCode,
                    FullName = model.FullName.Trim(),
                    Phone = model.Phone.Trim(),
                    Address = model.Address.Trim(),
                    BankAccountNo = model.BankAccountNo.Trim(),
                    BankName = model.BankName.Trim(),
                    IFSC = model.IFSC.Trim().ToUpperInvariant(),
                    SocietyID = model.SocietyID,
                    UserID = user.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _farmerRepository.Add(farmer);
                await _unitOfWork.SaveChangesAsync(ct); // farmer.FarmerID now populated

                _auditService.Log(nameof(Farmer), farmer.FarmerID, AuditAction.Created,
                    oldValue: null,
                    newValue: new { farmer.FarmerCode, farmer.FullName, farmer.Phone, farmer.SocietyID, LoginEmail = loginEmail },
                    performedByUserId);

                await _unitOfWork.SaveChangesAsync(ct); // persists the audit row

                await _unitOfWork.CommitTransactionAsync(ct);

                return new FarmerCredentialsViewModel
                {
                    FarmerID = farmer.FarmerID,
                    FullName = farmer.FullName,
                    FarmerCode = farmer.FarmerCode,
                    LoginEmail = loginEmail,
                    TemporaryPassword = temporaryPassword
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }

        public async Task UpdateAsync(FarmerFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var farmer = await _farmerRepository.GetByIdWithinSocietyAsync(model.FarmerID, model.SocietyID, ct)
                ?? throw new BusinessRuleException("Farmer not found in this society.");

            var farmerCode = model.FarmerCode.Trim();
            if (await _farmerRepository.FarmerCodeExistsInSocietyAsync(farmerCode, model.SocietyID, excludingFarmerId: farmer.FarmerID, ct))
            {
                throw new BusinessRuleException($"Farmer code '{farmerCode}' is already used in this society.");
            }

            var loginEmail = model.Email.Trim().ToLowerInvariant();
            if (farmer.User is null)
            {
                throw new BusinessRuleException("Farmer login account is missing.");
            }

            var existingWithEmail = await _userManager.FindByEmailAsync(loginEmail);
            if (existingWithEmail is not null && existingWithEmail.Id != farmer.UserID)
            {
                throw new BusinessRuleException($"Email '{loginEmail}' is already registered.");
            }

            var oldSnapshot = new
            {
                farmer.FarmerCode,
                farmer.FullName,
                farmer.Phone,
                farmer.BankAccountNo,
                farmer.BankName,
                farmer.IFSC,
                LoginEmail = farmer.User.Email
            };

            farmer.FarmerCode = farmerCode;
            farmer.FullName = model.FullName.Trim();
            farmer.Phone = model.Phone.Trim();
            farmer.Address = model.Address.Trim();
            farmer.BankAccountNo = model.BankAccountNo.Trim();
            farmer.BankName = model.BankName.Trim();
            farmer.IFSC = model.IFSC.Trim().ToUpperInvariant();

            if (!string.Equals(farmer.User.Email, loginEmail, StringComparison.OrdinalIgnoreCase))
            {
                farmer.User.Email = loginEmail;
                farmer.User.UserName = loginEmail;
                farmer.User.NormalizedEmail = _userManager.NormalizeEmail(loginEmail);
                farmer.User.NormalizedUserName = _userManager.NormalizeName(loginEmail);
            }

            _farmerRepository.SetOriginalRowVersion(farmer, model.RowVersion!);

            _auditService.Log(nameof(Farmer), farmer.FarmerID, AuditAction.Updated, oldSnapshot,
                new
                {
                    farmer.FarmerCode,
                    farmer.FullName,
                    farmer.Phone,
                    farmer.BankAccountNo,
                    farmer.BankName,
                    farmer.IFSC,
                    LoginEmail = loginEmail
                },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task SetActiveStatusAsync(int farmerId, int societyId, bool isActive, int performedByUserId, CancellationToken ct = default)
        {
            var farmer = await _farmerRepository.GetByIdWithinSocietyAsync(farmerId, societyId, ct)
                ?? throw new BusinessRuleException("Farmer not found in this society.");

            if (farmer.IsActive == isActive)
            {
                return;
            }

            farmer.IsActive = isActive;

            // Deactivating a farmer also locks their login out — soft-deleted
            // farmers shouldn't be able to log into the Farmer Portal.
            if (farmer.User is not null)
            {
                farmer.User.IsActive = isActive;
            }

            _auditService.Log(nameof(Farmer), farmer.FarmerID,
                isActive ? AuditAction.Activated : AuditAction.Deactivated,
                oldValue: new { IsActive = !isActive },
                newValue: new { IsActive = isActive },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
