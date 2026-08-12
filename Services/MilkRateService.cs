using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Services
{
    public class MilkRateService : IMilkRateService
    {
        private readonly IMilkRateRepository _milkRateRepository;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public MilkRateService(IMilkRateRepository milkRateRepository, IAuditService auditService, IUnitOfWork unitOfWork)
        {
            _milkRateRepository = milkRateRepository;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MilkRate>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await _milkRateRepository.GetBySocietyAsync(societyId, ct);
        }

        public async Task<MilkRate> CreateAsync(MilkRateFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var effectiveFrom = model.EffectiveFrom.Date;

            if (await _milkRateRepository.RateExistsAsync(model.SocietyID, model.FatPercent, effectiveFrom, excludingRateId: null, ct))
            {
                throw new BusinessRuleException(
                    $"A rate for {model.FatPercent}% fat effective {effectiveFrom:dd-MMM-yyyy} already exists for this society.");
            }

            var rate = new MilkRate
            {
                SocietyID = model.SocietyID,
                FatPercent = model.FatPercent,
                RatePerLitre = model.RatePerLitre,
                EffectiveFrom = effectiveFrom,
                IsActive = true
            };

            _milkRateRepository.Add(rate);
            await _unitOfWork.SaveChangesAsync(ct); // rate.RateID now populated

            _auditService.Log(nameof(MilkRate), rate.RateID, AuditAction.Created,
                oldValue: null,
                newValue: new { rate.FatPercent, rate.RatePerLitre, rate.EffectiveFrom },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);

            return rate;
        }

        public async Task UpdateAsync(MilkRateFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var rate = await _milkRateRepository.GetByIdAsync(model.RateID, ct)
                ?? throw new BusinessRuleException("Rate not found.");

            if (rate.SocietyID != model.SocietyID)
            {
                // Defense in depth: this should never happen since the
                // controller always scopes lookups by the operator's own
                // society, but a mismatch here would mean something tried to
                // edit another society's rate — refuse outright rather than
                // silently proceeding.
                throw new BusinessRuleException("Rate does not belong to this society.");
            }

            var effectiveFrom = model.EffectiveFrom.Date;

            if (await _milkRateRepository.RateExistsAsync(model.SocietyID, model.FatPercent, effectiveFrom, excludingRateId: rate.RateID, ct))
            {
                throw new BusinessRuleException(
                    $"A rate for {model.FatPercent}% fat effective {effectiveFrom:dd-MMM-yyyy} already exists for this society.");
            }

            var oldSnapshot = new { rate.FatPercent, rate.RatePerLitre, rate.EffectiveFrom };

            rate.FatPercent = model.FatPercent;
            rate.RatePerLitre = model.RatePerLitre;
            rate.EffectiveFrom = effectiveFrom;

            _milkRateRepository.SetOriginalRowVersion(rate, model.RowVersion!);

            _auditService.Log(nameof(MilkRate), rate.RateID, AuditAction.Updated, oldSnapshot,
                new { rate.FatPercent, rate.RatePerLitre, rate.EffectiveFrom },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task SetActiveStatusAsync(int rateId, int societyId, bool isActive, int performedByUserId, CancellationToken ct = default)
        {
            var rate = await _milkRateRepository.GetByIdAsync(rateId, ct)
                ?? throw new BusinessRuleException("Rate not found.");

            if (rate.SocietyID != societyId)
            {
                throw new BusinessRuleException("Rate does not belong to this society.");
            }

            if (rate.IsActive == isActive)
            {
                return;
            }

            rate.IsActive = isActive;

            _auditService.Log(nameof(MilkRate), rate.RateID,
                isActive ? AuditAction.Activated : AuditAction.Deactivated,
                oldValue: new { IsActive = !isActive },
                newValue: new { IsActive = isActive },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task<MilkRate?> GetApplicableRateAsync(int societyId, decimal fatPercent, DateTime collectionDate, CancellationToken ct = default)
        {
            return await _milkRateRepository.GetApplicableRateAsync(societyId, fatPercent, collectionDate, ct);
        }
    }
}
