using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Services
{
    public class MilkCollectionService : IMilkCollectionService
    {
        private readonly IMilkCollectionRepository _collectionRepository;
        private readonly IFarmerRepository _farmerRepository;
        private readonly IMilkRateService _milkRateService;
        private readonly IShiftCloseService _shiftCloseService;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public MilkCollectionService(
            IMilkCollectionRepository collectionRepository,
            IFarmerRepository farmerRepository,
            IMilkRateService milkRateService,
            IShiftCloseService shiftCloseService,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _collectionRepository = collectionRepository;
            _farmerRepository = farmerRepository;
            _milkRateService = milkRateService;
            _shiftCloseService = shiftCloseService;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MilkCollection>> GetBySocietyAndDateAsync(int societyId, DateTime date, CancellationToken ct = default)
        {
            return await _collectionRepository.GetBySocietyAndDateAsync(societyId, date, ct);
        }

        public async Task<MilkCollection?> GetByIdWithinSocietyAsync(int collectionId, int societyId, CancellationToken ct = default)
        {
            return await _collectionRepository.GetByIdWithinSocietyAsync(collectionId, societyId, ct);
        }

        public async Task<MilkCollection?> GetByIdForFarmerAsync(int collectionId, int farmerId, CancellationToken ct = default)
        {
            return await _collectionRepository.GetByIdForFarmerAsync(collectionId, farmerId, ct);
        }

        public async Task<List<MilkCollection>> GetByFarmerAndDateRangeAsync(int farmerId, DateTime from, DateTime to, CancellationToken ct = default)
        {
            return await _collectionRepository.GetByFarmerAndDateRangeAsync(farmerId, from, to, ct);
        }

        public async Task<MilkCollection> CreateAsync(MilkCollectionFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var collectionDate = model.CollectionDate.Date;

            if (collectionDate > DateTime.Today)
            {
                // Not stated explicitly in the synopsis, but a reasonable
                // real-world rule: you can't record milk that hasn't been
                // delivered yet. Flagging this as an assumption, not a
                // silently invented hard requirement.
                throw new BusinessRuleException("Collection date cannot be in the future.");
            }

            // Ownership check: the selected farmer must actually belong to
            // this Operator's own society — never trust that the posted
            // FarmerID is legitimate just because it's a valid farmer SOMEWHERE.
            var farmer = await _farmerRepository.GetByIdAsync(model.FarmerID, ct);
            if (farmer is null || farmer.SocietyID != model.SocietyID)
            {
                throw new BusinessRuleException("Selected farmer does not belong to this society.");
            }

            if (!farmer.IsActive)
            {
                throw new BusinessRuleException("Selected farmer is inactive and cannot receive new collections.");
            }

            var existing = await _collectionRepository.GetByFarmerDateShiftAsync(
                model.FarmerID, collectionDate, model.Shift, excludingCollectionId: null, ct);

            if (existing is not null)
            {
                throw new BusinessRuleException(
                    $"A {model.Shift} collection for this farmer on {collectionDate:dd-MMM-yyyy} already exists. " +
                    "Edit the existing entry instead of creating a new one.");
            }

            await EnsureShiftOpenAsync(model.SocietyID, collectionDate, model.Shift, ct);

            var quantity = model.Quantity!.Value;
            var fatPercent = model.FatPercent!.Value;
            var snf = model.SNF!.Value;
            var clr = model.CLR!.Value;

            var (rate, amount, isRejected, rejectionReason) = await ResolvePaymentAsync(
                model, collectionDate, quantity, fatPercent, snf, clr, ct);

            var collection = new MilkCollection
            {
                FarmerID = model.FarmerID,
                SocietyID = model.SocietyID,
                CollectionDate = collectionDate,
                Shift = model.Shift,
                Quantity = quantity,
                FatPercent = fatPercent,
                SNF = snf,
                CLR = clr,
                RatePerLitre = rate,
                Amount = amount,
                IsRejected = isRejected,
                RejectionReason = rejectionReason,
                RecordedBy = performedByUserId,
                CreatedAt = DateTime.UtcNow,
                IsLocked = false
            };

            _collectionRepository.Add(collection);
            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                throw new BusinessRuleException(
                    $"A {model.Shift} collection for this farmer on {collectionDate:dd-MMM-yyyy} already exists. " +
                    "Edit the existing entry instead of creating a new one.");
            }

            _auditService.Log(nameof(MilkCollection), collection.CollectionID, AuditAction.Created,
                oldValue: null,
                newValue: new { collection.FarmerID, collection.CollectionDate, collection.Shift, collection.Quantity, collection.FatPercent, collection.RatePerLitre, collection.Amount, collection.IsRejected, collection.RejectionReason },
                performedByUserId,
                collection.SocietyID);

            await _unitOfWork.SaveChangesAsync(ct);

            return collection;
        }

        public async Task UpdateAsync(MilkCollectionFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var collection = await _collectionRepository.GetByIdWithinSocietyAsync(model.CollectionID, model.SocietyID, ct)
                ?? throw new BusinessRuleException("Collection entry not found.");

            if (collection.IsLocked)
            {
                // Enforces the synopsis's settlement-locking rule: once a
                // settlement has consumed this collection, it becomes
                // read-only for everyone except an Admin unlock (Stage 10).
                throw new BusinessRuleException(
                    "This collection is locked because it's part of a generated settlement and cannot be edited. Contact an Admin to unlock it if a correction is truly needed.");
            }

            await EnsureShiftOpenAsync(model.SocietyID, collection.CollectionDate, collection.Shift, ct);

            var farmer = await _farmerRepository.GetByIdAsync(collection.FarmerID, ct);
            if (farmer is null || !farmer.IsActive)
            {
                throw new BusinessRuleException("This farmer is inactive and their collections cannot be edited.");
            }

            var collectionDate = model.CollectionDate.Date;

            if (collectionDate > DateTime.Today)
            {
                throw new BusinessRuleException("Collection date cannot be in the future.");
            }

            // If the date or shift changed, re-check for a collision with a
            // DIFFERENT existing entry (excluding this one).
            var conflicting = await _collectionRepository.GetByFarmerDateShiftAsync(
                collection.FarmerID, collectionDate, model.Shift, excludingCollectionId: collection.CollectionID, ct);
            if (conflicting is not null)
            {
                throw new BusinessRuleException(
                    $"A {model.Shift} collection for this farmer on {collectionDate:dd-MMM-yyyy} already exists.");
            }

            await EnsureShiftOpenAsync(model.SocietyID, collectionDate, model.Shift, ct);

            var quantity = model.Quantity!.Value;
            var fatPercent = model.FatPercent!.Value;
            var snf = model.SNF!.Value;
            var clr = model.CLR!.Value;

            var (rate, amount, isRejected, rejectionReason) = await ResolvePaymentAsync(
                model, collectionDate, quantity, fatPercent, snf, clr, ct);

            var oldSnapshot = new { collection.CollectionDate, collection.Shift, collection.Quantity, collection.FatPercent, collection.RatePerLitre, collection.Amount, collection.IsRejected, collection.RejectionReason };

            collection.CollectionDate = collectionDate;
            collection.Shift = model.Shift;
            collection.Quantity = quantity;
            collection.FatPercent = fatPercent;
            collection.SNF = snf;
            collection.CLR = clr;
            collection.RatePerLitre = rate;
            collection.Amount = amount;
            collection.IsRejected = isRejected;
            collection.RejectionReason = rejectionReason;

            _collectionRepository.SetOriginalRowVersion(collection, model.RowVersion!);

            _auditService.Log(nameof(MilkCollection), collection.CollectionID, AuditAction.Updated, oldSnapshot,
                new { collection.CollectionDate, collection.Shift, collection.Quantity, collection.FatPercent, collection.RatePerLitre, collection.Amount, collection.IsRejected, collection.RejectionReason },
                performedByUserId,
                collection.SocietyID);

            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                throw new BusinessRuleException(
                    $"A {model.Shift} collection for this farmer on {collectionDate:dd-MMM-yyyy} already exists.");
            }
        }

        public async Task EnsureShiftOpenAsync(int societyId, DateTime date, Shift shift, CancellationToken ct = default)
        {
            if (await _shiftCloseService.IsClosedAsync(societyId, date, shift, ct))
            {
                throw new BusinessRuleException(
                    $"The {shift} shift on {date:dd-MMM-yyyy} is closed. Reopen it before recording or editing collections.");
            }
        }

        private async Task<(decimal Rate, decimal Amount, bool IsRejected, string? Reason)> ResolvePaymentAsync(
            MilkCollectionFormViewModel model,
            DateTime collectionDate,
            decimal quantity,
            decimal fatPercent,
            decimal snf,
            decimal clr,
            CancellationToken ct)
        {
            if (model.IsRejected)
            {
                var reason = (model.RejectionReason ?? string.Empty).Trim();
                if (reason.Length < 3)
                {
                    throw new BusinessRuleException("Enter a quality-rejection reason (at least 3 characters).");
                }

                return (0m, 0m, true, reason);
            }

            var applicableRate = await _milkRateService.GetApplicableRateAsync(model.SocietyID, fatPercent, snf, clr, collectionDate, ct);
            if (applicableRate is null)
            {
                throw new BusinessRuleException(
                    $"No applicable rate found for {fatPercent}% fat, {snf} SNF, {clr} CLR on {collectionDate:dd-MMM-yyyy}. " +
                    "Add a matching fat/SNF/CLR band on the Milk Rate Chart, or record this entry as a quality rejection.");
            }

            return (
                applicableRate.RatePerLitre,
                SettlementCalculator.ComputeCollectionAmount(quantity, applicableRate.RatePerLitre),
                false,
                null);
        }
    }
}
