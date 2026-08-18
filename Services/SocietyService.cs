using DairyManagementSystem.Data.Seed;
using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Services
{
    public class SocietyService : ISocietyService
    {
        private readonly ISocietyRepository _societyRepository;
        private readonly IMilkRateRepository _milkRateRepository;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public SocietyService(
            ISocietyRepository societyRepository,
            IMilkRateRepository milkRateRepository,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _societyRepository = societyRepository;
            _milkRateRepository = milkRateRepository;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Society>> GetAllAsync(CancellationToken ct = default)
        {
            var societies = await _societyRepository.GetAllAsync(ct);
            return societies.OrderBy(s => s.SocietyName).ToList();
        }

        public async Task<Society?> GetByIdAsync(int societyId, CancellationToken ct = default)
        {
            return await _societyRepository.GetByIdAsync(societyId, ct);
        }

        public async Task<string> GetNextRegistrationNoAsync(CancellationToken ct = default)
        {
            var numbers = await _societyRepository.GetRegistrationNumbersAsync(ct);
            return SequentialCode.Next(SequentialCode.SocietyPrefix, numbers, prefixOrNumericOnly: true);
        }

        public async Task<Society> CreateAsync(SocietyFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var registrationNo = await GetNextRegistrationNoAsync(ct);

            if (await _societyRepository.RegistrationNoExistsAsync(registrationNo, excludingSocietyId: null, ct))
            {
                throw new BusinessRuleException($"Registration number '{registrationNo}' is already in use by another society.");
            }

            var society = new Society
            {
                SocietyName = model.SocietyName.Trim(),
                RegistrationNo = registrationNo,
                Address = model.Address.Trim(),
                ContactPhone = model.ContactPhone.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _societyRepository.Add(society);

            // Must save once here first so society.SocietyID gets its real
            // database-generated value before the audit entry references it.
            await _unitOfWork.SaveChangesAsync(ct);

            foreach (var rate in DbInitializer.BuildRealisticRateChart(society.SocietyID))
            {
                _milkRateRepository.Add(rate);
            }

            _auditService.Log(nameof(Society), society.SocietyID, AuditAction.Created,
                oldValue: null,
                newValue: new { society.SocietyName, society.RegistrationNo, society.Address, society.ContactPhone },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct); // persists the audit row

            return society;
        }

        public async Task UpdateAsync(SocietyFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var society = await _societyRepository.GetByIdAsync(model.SocietyID, ct)
                ?? throw new BusinessRuleException("Society not found. It may have been removed by another user.");

            var oldSnapshot = new { society.SocietyName, society.RegistrationNo, society.Address, society.ContactPhone };

            society.SocietyName = model.SocietyName.Trim();
            society.Address = model.Address.Trim();
            society.ContactPhone = model.ContactPhone.Trim();

            // society is already tracked (loaded via GetByIdAsync in this same
            // request), so EF Core auto-detects the property changes above —
            // no need to call _societyRepository.Update() for an entity that's
            // already attached. We DO need this explicit call though: it tells
            // EF Core to check the DB against the RowVersion the edit form was
            // opened with, not the one just freshly loaded above.
            _societyRepository.SetOriginalRowVersion(society, model.RowVersion!);

            _auditService.Log(nameof(Society), society.SocietyID, AuditAction.Updated, oldSnapshot,
                new { society.SocietyName, society.RegistrationNo, society.Address, society.ContactPhone },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct); // throws DbUpdateConcurrencyException on conflict
        }

        public async Task SetActiveStatusAsync(int societyId, bool isActive, int performedByUserId, CancellationToken ct = default)
        {
            var society = await _societyRepository.GetByIdAsync(societyId, ct)
                ?? throw new BusinessRuleException("Society not found.");

            if (society.IsActive == isActive)
            {
                return; // no-op — already in the requested state
            }

            society.IsActive = isActive;

            _auditService.Log(nameof(Society), society.SocietyID,
                isActive ? AuditAction.Activated : AuditAction.Deactivated,
                oldValue: new { IsActive = !isActive },
                newValue: new { IsActive = isActive },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
