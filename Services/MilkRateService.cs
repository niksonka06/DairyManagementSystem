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
            var fatFrom = model.FatPercentFrom!.Value;
            var fatTo = model.FatPercentTo!.Value;
            var snfFrom = model.SnfPercentFrom!.Value;
            var snfTo = model.SnfPercentTo!.Value;
            var clrFrom = model.ClrFrom!.Value;
            var clrTo = model.ClrTo!.Value;
            var ratePerLitre = model.RatePerLitre!.Value;

            if (await _milkRateRepository.RangeOverlapsAsync(
                    model.SocietyID, fatFrom, fatTo, snfFrom, snfTo, clrFrom, clrTo, effectiveFrom, excludingRateId: null, ct))
            {
                throw new BusinessRuleException(
                    $"This fat/SNF/CLR range overlaps another rate effective {effectiveFrom:dd-MMM-yyyy} for this society.");
            }

            var rate = new MilkRate
            {
                SocietyID = model.SocietyID,
                FatPercentFrom = fatFrom,
                FatPercentTo = fatTo,
                SnfPercentFrom = snfFrom,
                SnfPercentTo = snfTo,
                ClrFrom = clrFrom,
                ClrTo = clrTo,
                RatePerLitre = ratePerLitre,
                EffectiveFrom = effectiveFrom,
                IsActive = true
            };

            _milkRateRepository.Add(rate);
            await _unitOfWork.SaveChangesAsync(ct);

            _auditService.Log(nameof(MilkRate), rate.RateID, AuditAction.Created,
                oldValue: null,
                newValue: RateSnapshot(rate),
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
                throw new BusinessRuleException("Rate does not belong to this society.");
            }

            var effectiveFrom = model.EffectiveFrom.Date;
            var fatFrom = model.FatPercentFrom!.Value;
            var fatTo = model.FatPercentTo!.Value;
            var snfFrom = model.SnfPercentFrom!.Value;
            var snfTo = model.SnfPercentTo!.Value;
            var clrFrom = model.ClrFrom!.Value;
            var clrTo = model.ClrTo!.Value;
            var ratePerLitre = model.RatePerLitre!.Value;

            if (await _milkRateRepository.RangeOverlapsAsync(
                    model.SocietyID, fatFrom, fatTo, snfFrom, snfTo, clrFrom, clrTo, effectiveFrom, excludingRateId: rate.RateID, ct))
            {
                throw new BusinessRuleException(
                    $"This fat/SNF/CLR range overlaps another rate effective {effectiveFrom:dd-MMM-yyyy} for this society.");
            }

            var oldSnapshot = RateSnapshot(rate);

            rate.FatPercentFrom = fatFrom;
            rate.FatPercentTo = fatTo;
            rate.SnfPercentFrom = snfFrom;
            rate.SnfPercentTo = snfTo;
            rate.ClrFrom = clrFrom;
            rate.ClrTo = clrTo;
            rate.RatePerLitre = ratePerLitre;
            rate.EffectiveFrom = effectiveFrom;

            _milkRateRepository.SetOriginalRowVersion(rate, model.RowVersion!);

            _auditService.Log(nameof(MilkRate), rate.RateID, AuditAction.Updated, oldSnapshot,
                RateSnapshot(rate),
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

        public async Task<MilkRate?> GetApplicableRateAsync(
            int societyId,
            decimal fatPercent,
            decimal snf,
            decimal clr,
            DateTime collectionDate,
            CancellationToken ct = default)
        {
            return await _milkRateRepository.GetApplicableRateAsync(societyId, fatPercent, snf, clr, collectionDate, ct);
        }

        private static object RateSnapshot(MilkRate rate) => new
        {
            rate.FatPercentFrom,
            rate.FatPercentTo,
            rate.SnfPercentFrom,
            rate.SnfPercentTo,
            rate.ClrFrom,
            rate.ClrTo,
            rate.RatePerLitre,
            rate.EffectiveFrom
        };
    }
}
