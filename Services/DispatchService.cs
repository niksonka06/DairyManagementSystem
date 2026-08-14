using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.Extensions.Configuration;

namespace DairyManagementSystem.Services
{
    public class DispatchService : IDispatchService
    {
        private readonly IDispatchRepository _dispatchRepository;
        private readonly IMilkCollectionRepository _collectionRepository;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public DispatchService(
            IDispatchRepository dispatchRepository,
            IMilkCollectionRepository collectionRepository,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _dispatchRepository = dispatchRepository;
            _collectionRepository = collectionRepository;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<List<Dispatch>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await _dispatchRepository.GetBySocietyAsync(societyId, ct);
        }

        public async Task<Dispatch?> GetByIdWithinSocietyAsync(int dispatchId, int societyId, CancellationToken ct = default)
        {
            return await _dispatchRepository.GetByIdWithinSocietyAsync(dispatchId, societyId, ct);
        }

        public async Task<Dispatch> CreateAsync(DispatchFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var date = model.DispatchDate.Date;

            if (await _dispatchRepository.ExistsForDateAsync(model.SocietyID, date, excludingDispatchId: null, ct))
            {
                throw new BusinessRuleException($"A dispatch record for {date:dd-MMM-yyyy} already exists for this society.");
            }

            var totalCollected = await _collectionRepository.GetTotalQuantityBySocietyAndDateAsync(model.SocietyID, date, ct);

            EnsureVarianceReasonIfNeeded(totalCollected, model.TotalDispatched, model.VarianceReason);

            var dispatch = new Dispatch
            {
                SocietyID = model.SocietyID,
                DispatchDate = date,
                DispatchTime = model.DispatchTime,
                VehicleNo = model.VehicleNo.Trim(),
                Destination = model.Destination.Trim(),
                TotalCollected = totalCollected,
                TotalDispatched = model.TotalDispatched,
                VarianceReason = string.IsNullOrWhiteSpace(model.VarianceReason) ? null : model.VarianceReason.Trim(),
                RecordedBy = performedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _dispatchRepository.Add(dispatch);
            await _unitOfWork.SaveChangesAsync(ct);

            _auditService.Log(nameof(Dispatch), dispatch.DispatchID, AuditAction.Created,
                oldValue: null,
                newValue: new { dispatch.DispatchDate, dispatch.TotalCollected, dispatch.TotalDispatched, Variance = dispatch.Variance },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);

            return dispatch;
        }

        public async Task UpdateAsync(DispatchFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var dispatch = await _dispatchRepository.GetByIdWithinSocietyAsync(model.DispatchID, model.SocietyID, ct)
                ?? throw new BusinessRuleException("Dispatch record not found.");

            var date = model.DispatchDate.Date;

            if (await _dispatchRepository.ExistsForDateAsync(model.SocietyID, date, excludingDispatchId: dispatch.DispatchID, ct))
            {
                throw new BusinessRuleException($"A dispatch record for {date:dd-MMM-yyyy} already exists for this society.");
            }

            // Recomputed on every edit — reflects the latest collection data
            // for that date, since Edit implies "fix this record", not
            // "preserve whatever was true at creation time" (unlike a
            // farmer's locked settlement amount, dispatch totals aren't a
            // payment promise to anyone).
            var totalCollected = await _collectionRepository.GetTotalQuantityBySocietyAndDateAsync(model.SocietyID, date, ct);

            EnsureVarianceReasonIfNeeded(totalCollected, model.TotalDispatched, model.VarianceReason);

            var oldSnapshot = new { dispatch.DispatchDate, dispatch.TotalCollected, dispatch.TotalDispatched };

            dispatch.DispatchDate = date;
            dispatch.DispatchTime = model.DispatchTime;
            dispatch.VehicleNo = model.VehicleNo.Trim();
            dispatch.Destination = model.Destination.Trim();
            dispatch.TotalCollected = totalCollected;
            dispatch.TotalDispatched = model.TotalDispatched;
            dispatch.VarianceReason = string.IsNullOrWhiteSpace(model.VarianceReason) ? null : model.VarianceReason.Trim();

            _dispatchRepository.SetOriginalRowVersion(dispatch, model.RowVersion!);

            _auditService.Log(nameof(Dispatch), dispatch.DispatchID, AuditAction.Updated, oldSnapshot,
                new { dispatch.DispatchDate, dispatch.TotalCollected, dispatch.TotalDispatched, Variance = dispatch.Variance },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        private void EnsureVarianceReasonIfNeeded(decimal totalCollected, decimal totalDispatched, string? reason)
        {
            var variance = totalCollected - totalDispatched;
            var variancePercent = totalCollected == 0 ? 0 : Math.Abs(variance) / totalCollected * 100;

            // Config value in appsettings.json, not hardcoded — the synopsis
            // calls this a "configured threshold" without giving a number
            // (flagged back in Stage 0). Falls back to 5% if unset/zero.
            var thresholdPercent = _configuration.GetValue<decimal?>("AppSettings:DispatchVarianceThresholdPercent");
            if (thresholdPercent is null or <= 0)
            {
                thresholdPercent = 5m;
            }

            if (variancePercent > thresholdPercent && string.IsNullOrWhiteSpace(reason))
            {
                throw new BusinessRuleException(
                    $"Variance is {variancePercent:0.0}% (collected {totalCollected:0.00}L, dispatched {totalDispatched:0.00}L), " +
                    $"which exceeds the {thresholdPercent:0.0}% threshold. A reason is required.");
            }
        }
    }
}
