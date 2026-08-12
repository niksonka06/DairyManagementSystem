using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Services
{
    public class MilkCollectionService : IMilkCollectionService
    {
        private readonly IMilkCollectionRepository _collectionRepository;
        private readonly IFarmerRepository _farmerRepository;
        private readonly IMilkRateService _milkRateService;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public MilkCollectionService(
            IMilkCollectionRepository collectionRepository,
            IFarmerRepository farmerRepository,
            IMilkRateService milkRateService,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _collectionRepository = collectionRepository;
            _farmerRepository = farmerRepository;
            _milkRateService = milkRateService;
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

            var existing = await _collectionRepository.GetByFarmerDateShiftAsync(
                model.FarmerID, collectionDate, model.Shift, excludingCollectionId: null, ct);

            if (existing is not null)
            {
                throw new BusinessRuleException(
                    $"A {model.Shift} collection for this farmer on {collectionDate:dd-MMM-yyyy} already exists. " +
                    "Edit the existing entry instead of creating a new one.");
            }

            var applicableRate = await _milkRateService.GetApplicableRateAsync(model.SocietyID, model.FatPercent, collectionDate, ct);
            if (applicableRate is null)
            {
                throw new BusinessRuleException(
                    $"No applicable rate found for {model.FatPercent}% fat on {collectionDate:dd-MMM-yyyy}. " +
                    "Check the Milk Rate Chart for this society.");
            }

            var collection = new MilkCollection
            {
                FarmerID = model.FarmerID,
                SocietyID = model.SocietyID,
                CollectionDate = collectionDate,
                Shift = model.Shift,
                Quantity = model.Quantity,
                FatPercent = model.FatPercent,
                SNF = model.SNF,
                CLR = model.CLR,
                RatePerLitre = applicableRate.RatePerLitre, // snapshot — see class comment on MilkCollection
                Amount = model.Quantity * applicableRate.RatePerLitre,
                RecordedBy = performedByUserId,
                CreatedAt = DateTime.UtcNow,
                IsLocked = false
            };

            _collectionRepository.Add(collection);
            await _unitOfWork.SaveChangesAsync(ct); // collection.CollectionID now populated

            _auditService.Log(nameof(MilkCollection), collection.CollectionID, AuditAction.Created,
                oldValue: null,
                newValue: new { collection.FarmerID, collection.CollectionDate, collection.Shift, collection.Quantity, collection.FatPercent, collection.RatePerLitre, collection.Amount },
                performedByUserId);

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

            // Fat% (and possibly date) may have changed via this correction —
            // re-resolve the rate rather than keeping the original, since a
            // wrong fat% entry would have produced a wrong rate the first
            // time. This is a deliberate design choice: a genuine correction
            // should correct its downstream effects too, not just the raw
            // input field. (Once locked by settlement, none of this is
            // reachable anyway — see the IsLocked check above.)
            var applicableRate = await _milkRateService.GetApplicableRateAsync(model.SocietyID, model.FatPercent, collectionDate, ct);
            if (applicableRate is null)
            {
                throw new BusinessRuleException(
                    $"No applicable rate found for {model.FatPercent}% fat on {collectionDate:dd-MMM-yyyy}.");
            }

            var oldSnapshot = new { collection.CollectionDate, collection.Shift, collection.Quantity, collection.FatPercent, collection.RatePerLitre, collection.Amount };

            collection.CollectionDate = collectionDate;
            collection.Shift = model.Shift;
            collection.Quantity = model.Quantity;
            collection.FatPercent = model.FatPercent;
            collection.SNF = model.SNF;
            collection.CLR = model.CLR;
            collection.RatePerLitre = applicableRate.RatePerLitre;
            collection.Amount = model.Quantity * applicableRate.RatePerLitre;

            _collectionRepository.SetOriginalRowVersion(collection, model.RowVersion!);

            _auditService.Log(nameof(MilkCollection), collection.CollectionID, AuditAction.Updated, oldSnapshot,
                new { collection.CollectionDate, collection.Shift, collection.Quantity, collection.FatPercent, collection.RatePerLitre, collection.Amount },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
