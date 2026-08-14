using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ISettlementDeductionRepository _deductionRepository;
        private readonly IAdvancePaymentRepository _advancePaymentRepository;
        private readonly IMilkCollectionRepository _collectionRepository;
        private readonly IFeedIssueRepository _feedIssueRepository;
        private readonly IFarmerRepository _farmerRepository;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISmsService _smsService;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ISettlementDeductionRepository deductionRepository,
            IAdvancePaymentRepository advancePaymentRepository,
            IMilkCollectionRepository collectionRepository,
            IFeedIssueRepository feedIssueRepository,
            IFarmerRepository farmerRepository,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            ISmsService smsService)
        {
            _paymentRepository = paymentRepository;
            _deductionRepository = deductionRepository;
            _advancePaymentRepository = advancePaymentRepository;
            _collectionRepository = collectionRepository;
            _feedIssueRepository = feedIssueRepository;
            _farmerRepository = farmerRepository;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
            _smsService = smsService;
        }

        public async Task<List<Payment>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await _paymentRepository.GetBySocietyAsync(societyId, ct);
        }

        public async Task<Payment?> GetByIdWithinSocietyAsync(int paymentId, int societyId, CancellationToken ct = default)
        {
            return await _paymentRepository.GetByIdWithinSocietyAsync(paymentId, societyId, ct);
        }

        public async Task<Payment> CreateDraftAsync(SettlementCreateViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var farmer = await _farmerRepository.GetByIdAsync(model.FarmerID, ct);
            if (farmer is null || farmer.SocietyID != model.SocietyID)
            {
                throw new BusinessRuleException("Selected farmer does not belong to this society.");
            }

            var (periodStart, periodEnd) = DateHelpers.ComputeWeek(model.WeekReferenceDate);

            if (await _paymentRepository.ExistsNonCancelledForPeriodAsync(model.FarmerID, periodStart, excludingPaymentId: null, ct))
            {
                throw new BusinessRuleException(
                    $"A settlement for this farmer covering {periodStart:dd-MMM-yyyy} to {periodEnd:dd-MMM-yyyy} already exists.");
            }

            var (gross, feedDeduction, medicineDeduction) = await ComputeAutoAmountsAsync(model.FarmerID, periodStart, periodEnd, ct);

            if (gross == 0)
            {
                throw new BusinessRuleException(
                    $"No unlocked collections found for this farmer between {periodStart:dd-MMM-yyyy} and {periodEnd:dd-MMM-yyyy}.");
            }

            var advancePaid = (await _advancePaymentRepository.GetUnappliedByFarmerAsync(model.FarmerID, ct)).Sum(a => a.Amount);

            var otherDeductionsTotal = model.LoanDeduction + model.InsuranceDeduction + model.SocietyFeeDeduction + model.OtherDeduction;

            var payment = new Payment
            {
                FarmerID = model.FarmerID,
                SocietyID = model.SocietyID,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                GrossAmount = gross,
                FeedDeduction = feedDeduction,
                MedicineDeduction = medicineDeduction,
                OtherDeductionsTotal = otherDeductionsTotal,
                PreviousDue = model.PreviousDue,
                AdvancePaid = advancePaid,
                NetAmount = ComputeNet(gross, feedDeduction, medicineDeduction, otherDeductionsTotal, model.PreviousDue, advancePaid),
                Status = SettlementStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            _paymentRepository.Add(payment);
            await _unitOfWork.SaveChangesAsync(ct); // payment.PaymentID now populated

            AddDeductionLines(payment.PaymentID, model);
            await _unitOfWork.SaveChangesAsync(ct);

            _auditService.Log(nameof(Payment), payment.PaymentID, AuditAction.Created,
                oldValue: null,
                newValue: new { payment.FarmerID, payment.PeriodStart, payment.PeriodEnd, payment.GrossAmount, payment.NetAmount },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);

            return payment;
        }

        public async Task RecalculateAsync(int paymentId, int societyId, CancellationToken ct = default)
        {
            var payment = await _paymentRepository.GetByIdWithinSocietyAsync(paymentId, societyId, ct)
                ?? throw new BusinessRuleException("Settlement not found.");

            if (payment.Status != SettlementStatus.Draft)
            {
                throw new BusinessRuleException("Only Draft settlements can be recalculated.");
            }

            var (gross, feedDeduction, medicineDeduction) = await ComputeAutoAmountsAsync(payment.FarmerID, payment.PeriodStart, payment.PeriodEnd, ct);
            var advancePaid = (await _advancePaymentRepository.GetUnappliedByFarmerAsync(payment.FarmerID, ct)).Sum(a => a.Amount);

            payment.GrossAmount = gross;
            payment.FeedDeduction = feedDeduction;
            payment.MedicineDeduction = medicineDeduction;
            payment.AdvancePaid = advancePaid;
            payment.NetAmount = ComputeNet(gross, feedDeduction, medicineDeduction, payment.OtherDeductionsTotal, payment.PreviousDue, advancePaid);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task GenerateAsync(int paymentId, int societyId, int performedByUserId, CancellationToken ct = default)
        {
            var payment = await _paymentRepository.GetByIdWithinSocietyAsync(paymentId, societyId, ct)
                ?? throw new BusinessRuleException("Settlement not found.");

            if (payment.Status != SettlementStatus.Draft)
            {
                throw new BusinessRuleException("Only Draft settlements can be generated.");
            }

            // Final, authoritative recompute right before locking anything —
            // protects against stale figures if the operator never clicked
            // Recalculate after the Draft was first created.
            var (gross, feedDeduction, medicineDeduction) = await ComputeAutoAmountsAsync(payment.FarmerID, payment.PeriodStart, payment.PeriodEnd, ct);
            if (gross == 0)
            {
                throw new BusinessRuleException("No unlocked collections remain for this farmer/period — nothing to generate.");
            }

            var unappliedAdvances = await _advancePaymentRepository.GetUnappliedByFarmerAsync(payment.FarmerID, ct);
            var advancePaid = unappliedAdvances.Sum(a => a.Amount);

            payment.GrossAmount = gross;
            payment.FeedDeduction = feedDeduction;
            payment.MedicineDeduction = medicineDeduction;
            payment.AdvancePaid = advancePaid;
            payment.NetAmount = ComputeNet(gross, feedDeduction, medicineDeduction, payment.OtherDeductionsTotal, payment.PreviousDue, advancePaid);

            // LOCK every collection and feed issue this settlement consumed —
            // the synopsis's core settlement-locking rule. Once Generated,
            // none of these can be edited except through an Admin unlock.
            var collections = await _collectionRepository.GetUnlockedByFarmerAndPeriodAsync(payment.FarmerID, payment.PeriodStart, payment.PeriodEnd, ct);
            foreach (var c in collections)
            {
                c.IsLocked = true;
                c.LockedBySettlementID = payment.PaymentID;
            }

            var issues = await _feedIssueRepository.GetUnlockedByFarmerAndPeriodAsync(payment.FarmerID, payment.PeriodStart, payment.PeriodEnd, ct);
            foreach (var i in issues)
            {
                i.IsLocked = true;
                i.LockedBySettlementID = payment.PaymentID;
            }

            foreach (var advance in unappliedAdvances)
            {
                advance.IsApplied = true;
                advance.AppliedToPaymentID = payment.PaymentID;
            }

            payment.Status = SettlementStatus.Generated;
            payment.GeneratedBy = performedByUserId;
            payment.GeneratedAt = DateTime.UtcNow;

            _auditService.Log(nameof(Payment), payment.PaymentID, AuditAction.SettlementGenerated,
                oldValue: null,
                newValue: new { payment.NetAmount, CollectionsLocked = collections.Count, IssuesLocked = issues.Count, AdvancesApplied = unappliedAdvances.Count },
                performedByUserId);

            // One SaveChangesAsync commits ALL of this together — payment
            // status, every locked collection, every locked issue, every
            // applied advance, and the audit entry. If anything here throws,
            // NONE of it persists (EF Core's automatic per-SaveChanges
            // transaction) — exactly the "commit transaction; rollback
            // everything on failure" rule the synopsis requires for
            // settlement generation.
            await _unitOfWork.SaveChangesAsync(ct);

            // Fired AFTER the settlement is durably saved, not before — this
            // ordering is deliberate. The SMS is a notification about a
            // committed fact, never a precondition for it. Even if this
            // fails, the settlement itself remains validly Generated.
            var farmer = await _farmerRepository.GetByIdAsync(payment.FarmerID, ct);
            if (farmer is not null && !string.IsNullOrWhiteSpace(farmer.Phone))
            {
                var message = $"Dairy Co-op: Your settlement for {payment.PeriodStart:dd-MMM} to {payment.PeriodEnd:dd-MMM} is ready. Net amount: Rs.{payment.NetAmount:0.00}.";
                await _smsService.SendAsync(farmer.Phone, message, ct);
            }
        }

        public async Task MarkPaidAsync(int paymentId, int societyId, int performedByUserId, CancellationToken ct = default)
        {
            var payment = await _paymentRepository.GetByIdWithinSocietyAsync(paymentId, societyId, ct)
                ?? throw new BusinessRuleException("Settlement not found.");

            if (payment.Status != SettlementStatus.Generated)
            {
                throw new BusinessRuleException("Only Generated settlements can be marked Paid.");
            }

            payment.Status = SettlementStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;

            _auditService.Log(nameof(Payment), payment.PaymentID, AuditAction.Updated,
                oldValue: new { Status = SettlementStatus.Generated },
                newValue: new { Status = SettlementStatus.Paid },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task CancelDraftAsync(int paymentId, int societyId, int performedByUserId, CancellationToken ct = default)
        {
            var payment = await _paymentRepository.GetByIdWithinSocietyAsync(paymentId, societyId, ct)
                ?? throw new BusinessRuleException("Settlement not found.");

            if (payment.Status != SettlementStatus.Draft)
            {
                throw new BusinessRuleException("Only Draft settlements can be cancelled this way. A Generated settlement requires an Admin unlock.");
            }

            // A Draft never locked anything, so cancelling it is simple —
            // no unlock step needed, no reason required.
            payment.Status = SettlementStatus.Cancelled;
            payment.CancelledBy = performedByUserId;
            payment.CancelledAt = DateTime.UtcNow;

            _auditService.Log(nameof(Payment), payment.PaymentID, AuditAction.Deleted,
                oldValue: new { Status = SettlementStatus.Draft },
                newValue: new { Status = SettlementStatus.Cancelled },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task<List<Payment>> GetGeneratedAcrossAllSocietiesAsync(CancellationToken ct = default)
        {
            return await _paymentRepository.GetGeneratedAcrossAllSocietiesAsync(ct);
        }

        public async Task CancelGeneratedAsync(int paymentId, int performedByUserId, string reason, CancellationToken ct = default)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, ct)
                ?? throw new BusinessRuleException("Settlement not found.");

            if (payment.Status != SettlementStatus.Generated)
            {
                throw new BusinessRuleException("Only Generated settlements can be unlocked/cancelled this way.");
            }

            // UNLOCK everything this settlement had locked — this is the
            // synopsis's "administrator unlocks a collection" event. Requires
            // a reason (enforced by [Required] on SettlementUnlockViewModel)
            // and is fully audited below.
            var collections = await _collectionRepository.GetLockedBySettlementAsync(payment.PaymentID, ct);
            foreach (var c in collections)
            {
                c.IsLocked = false;
                c.LockedBySettlementID = null;
            }

            var issues = await _feedIssueRepository.GetLockedBySettlementAsync(payment.PaymentID, ct);
            foreach (var i in issues)
            {
                i.IsLocked = false;
                i.LockedBySettlementID = null;
            }

            var advances = await _advancePaymentRepository.GetAppliedToPaymentAsync(payment.PaymentID, ct);
            foreach (var a in advances)
            {
                a.IsApplied = false;
                a.AppliedToPaymentID = null;
            }

            payment.Status = SettlementStatus.Cancelled;
            payment.CancellationReason = reason;
            payment.CancelledBy = performedByUserId;
            payment.CancelledAt = DateTime.UtcNow;

            _auditService.Log(nameof(Payment), payment.PaymentID, AuditAction.Unlocked,
                oldValue: new { Status = SettlementStatus.Generated },
                newValue: new { Status = SettlementStatus.Cancelled, Reason = reason, CollectionsUnlocked = collections.Count, IssuesUnlocked = issues.Count },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        // ── Shared helpers ──────────────────────────────────────────────

        private async Task<(decimal Gross, decimal FeedDeduction, decimal MedicineDeduction)> ComputeAutoAmountsAsync(
            int farmerId, DateTime periodStart, DateTime periodEnd, CancellationToken ct)
        {
            var collections = await _collectionRepository.GetUnlockedByFarmerAndPeriodAsync(farmerId, periodStart, periodEnd, ct);
            var gross = collections.Sum(c => c.Amount);

            var issues = await _feedIssueRepository.GetUnlockedByFarmerAndPeriodAsync(farmerId, periodStart, periodEnd, ct);
            var feedDeduction = issues.Where(i => i.ItemType == ItemType.Feed).Sum(i => i.TotalCost);
            var medicineDeduction = issues.Where(i => i.ItemType == ItemType.Medicine).Sum(i => i.TotalCost);

            return (gross, feedDeduction, medicineDeduction);
        }

        private static decimal ComputeNet(decimal gross, decimal feedDeduction, decimal medicineDeduction, decimal otherDeductionsTotal, decimal previousDue, decimal advancePaid)
        {
            // The exact formula from the synopsis:
            // GrossAmount - FeedDeduction - MedicineDeduction - OtherDeductions + PreviousDue - AdvancePaid = NetAmount
            return gross - feedDeduction - medicineDeduction - otherDeductionsTotal + previousDue - advancePaid;
        }

        private void AddDeductionLines(int paymentId, SettlementCreateViewModel model)
        {
            void AddIfNonZero(DeductionType type, decimal amount)
            {
                if (amount > 0)
                {
                    _deductionRepository.Add(new SettlementDeduction { PaymentID = paymentId, DeductionType = type, Amount = amount });
                }
            }

            AddIfNonZero(DeductionType.Loan, model.LoanDeduction);
            AddIfNonZero(DeductionType.Insurance, model.InsuranceDeduction);
            AddIfNonZero(DeductionType.SocietyFee, model.SocietyFeeDeduction);
            AddIfNonZero(DeductionType.Other, model.OtherDeduction);
        }
    }
}
