using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DairyManagementSystem.Tests
{
    internal sealed class MemoryStore
    {
        public List<Farmer> Farmers { get; } = new();
        public List<MilkCollection> Collections { get; } = new();
        public List<MilkRate> Rates { get; } = new();
        public List<FeedInventory> Inventory { get; } = new();
        public List<FeedIssue> Issues { get; } = new();
        public List<Payment> Payments { get; } = new();
        public List<SettlementDeduction> Deductions { get; } = new();
        public List<AdvancePayment> Advances { get; } = new();
        public List<Dispatch> Dispatches { get; } = new();

        private int _farmer = 1, _collection = 1, _rate = 1, _item = 1, _issue = 1, _payment = 1, _deduction = 1, _advance = 1, _dispatch = 1;

        public void AssignIds()
        {
            foreach (var x in Farmers.Where(x => x.FarmerID == 0)) x.FarmerID = _farmer++;
            foreach (var x in Collections.Where(x => x.CollectionID == 0)) x.CollectionID = _collection++;
            foreach (var x in Rates.Where(x => x.RateID == 0)) x.RateID = _rate++;
            foreach (var x in Inventory.Where(x => x.FeedItemID == 0)) x.FeedItemID = _item++;
            foreach (var x in Issues.Where(x => x.IssueID == 0)) x.IssueID = _issue++;
            foreach (var x in Payments.Where(x => x.PaymentID == 0)) x.PaymentID = _payment++;
            foreach (var x in Deductions.Where(x => x.SettlementDeductionID == 0)) x.SettlementDeductionID = _deduction++;
            foreach (var x in Advances.Where(x => x.AdvancePaymentID == 0)) x.AdvancePaymentID = _advance++;
            foreach (var x in Dispatches.Where(x => x.DispatchID == 0)) x.DispatchID = _dispatch++;
        }
    }

    internal sealed class FakeUnitOfWork : IUnitOfWork
    {
        private readonly MemoryStore _store;
        public bool ThrowConcurrency { get; set; }

        public FakeUnitOfWork(MemoryStore store) => _store = store;

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            if (ThrowConcurrency)
            {
                throw new DbUpdateConcurrencyException("RowVersion conflict");
            }

            _store.AssignIds();
            return Task.FromResult(1);
        }

        public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    internal sealed class FakeAuditService : IAuditService
    {
        public void Log(string entityType, int entityId, AuditAction action, object? oldValue, object? newValue, int performedByUserId, int? societyId = null)
        {
        }
    }

    internal sealed class FakeSmsService : ISmsService
    {
        public List<string> Messages { get; } = new();

        public Task SendAsync(string phoneNumber, string message, CancellationToken ct = default)
        {
            Messages.Add($"{phoneNumber}:{message}");
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeShiftCloseService : IShiftCloseService
    {
        public HashSet<(int SocietyId, DateTime Date, Shift Shift)> Closed { get; } = new();

        public Task<bool> IsClosedAsync(int societyId, DateTime date, Shift shift, CancellationToken ct = default)
            => Task.FromResult(Closed.Contains((societyId, date.Date, shift)));

        public Task<IReadOnlyCollection<Shift>> GetClosedShiftsAsync(int societyId, DateTime date, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyCollection<Shift>>(Closed.Where(c => c.SocietyId == societyId && c.Date == date.Date).Select(c => c.Shift).ToList());

        public Task CloseAsync(int societyId, DateTime date, Shift shift, int performedByUserId, CancellationToken ct = default)
        {
            Closed.Add((societyId, date.Date, shift));
            return Task.CompletedTask;
        }

        public Task ReopenAsync(int societyId, DateTime date, Shift shift, int performedByUserId, CancellationToken ct = default)
        {
            Closed.Remove((societyId, date.Date, shift));
            return Task.CompletedTask;
        }

        public Task<bool> CanReopenAsync(int societyId, DateTime date, Shift shift, CancellationToken ct = default)
            => IsClosedAsync(societyId, date, shift, ct);
    }

    internal abstract class ListRepo<T> : IRepository<T> where T : class
    {
        protected readonly List<T> Items;

        protected ListRepo(List<T> items) => Items = items;

        public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var match = Items.FirstOrDefault(e => Convert.ToInt32(e.GetType().GetProperty(IdName)!.GetValue(e)) == id);
            return Task.FromResult(match);
        }

        public Task<List<T>> GetAllAsync(CancellationToken ct = default) => Task.FromResult(Items.ToList());
        public void Add(T entity) => Items.Add(entity);
        public void Update(T entity) { }
        public void Remove(T entity) => Items.Remove(entity);
        public virtual void SetOriginalRowVersion(T entity, byte[] rowVersion) { }

        protected abstract string IdName { get; }
    }

    internal sealed class FakeFarmerRepository : ListRepo<Farmer>, IFarmerRepository
    {
        public FakeFarmerRepository(MemoryStore store) : base(store.Farmers) { }
        protected override string IdName => nameof(Farmer.FarmerID);

        public Task<bool> FarmerCodeExistsInSocietyAsync(string farmerCode, int societyId, int? excludingFarmerId, CancellationToken ct = default)
            => Task.FromResult(Items.Any(f => f.SocietyID == societyId && f.FarmerCode == farmerCode && f.FarmerID != (excludingFarmerId ?? 0)));

        public Task<List<string>> GetCodesBySocietyAsync(int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(f => f.SocietyID == societyId).Select(f => f.FarmerCode).ToList());

        public Task<List<Farmer>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(f => f.SocietyID == societyId).ToList());

        public Task<Farmer?> GetByIdWithinSocietyAsync(int farmerId, int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(f => f.FarmerID == farmerId && f.SocietyID == societyId));

        public Task<Farmer?> GetByUserIdAsync(int userId, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(f => f.UserID == userId));
    }

    internal sealed class FakeCollectionRepository : ListRepo<MilkCollection>, IMilkCollectionRepository
    {
        public FakeCollectionRepository(MemoryStore store) : base(store.Collections) { }
        protected override string IdName => nameof(MilkCollection.CollectionID);

        public Task<List<MilkCollection>> GetBySocietyAndDateAsync(int societyId, DateTime date, CancellationToken ct = default)
            => Task.FromResult(Items.Where(c => c.SocietyID == societyId && c.CollectionDate == date.Date).ToList());

        public Task<MilkCollection?> GetByFarmerDateShiftAsync(int farmerId, DateTime date, Shift shift, int? excludingCollectionId, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(c =>
                c.FarmerID == farmerId && c.CollectionDate == date.Date && c.Shift == shift && c.CollectionID != (excludingCollectionId ?? 0)));

        public Task<MilkCollection?> GetByIdWithinSocietyAsync(int collectionId, int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(c => c.CollectionID == collectionId && c.SocietyID == societyId));

        public Task<MilkCollection?> GetByIdForFarmerAsync(int collectionId, int farmerId, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(c => c.CollectionID == collectionId && c.FarmerID == farmerId));

        public Task<List<MilkCollection>> GetUnlockedByFarmerAndPeriodAsync(int farmerId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
            => Task.FromResult(Items.Where(c => c.FarmerID == farmerId && !c.IsLocked && !c.IsRejected
                && c.CollectionDate >= periodStart.Date && c.CollectionDate <= periodEnd.Date).ToList());

        public Task<List<MilkCollection>> GetLockedBySettlementAsync(int paymentId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(c => c.LockedBySettlementID == paymentId).ToList());

        public Task<decimal> GetTotalQuantityBySocietyAndDateAsync(int societyId, DateTime date, CancellationToken ct = default)
            => Task.FromResult(Items.Where(c => c.SocietyID == societyId && c.CollectionDate == date.Date && !c.IsRejected).Sum(c => c.Quantity));

        public Task<List<MilkCollection>> GetBySocietyAndDateRangeAsync(int societyId, DateTime from, DateTime to, CancellationToken ct = default)
            => Task.FromResult(Items.Where(c => c.SocietyID == societyId && c.CollectionDate >= from.Date && c.CollectionDate <= to.Date).ToList());

        public Task<List<MilkCollection>> GetByFarmerAndDateRangeAsync(int farmerId, DateTime from, DateTime to, CancellationToken ct = default)
            => Task.FromResult(Items.Where(c => c.FarmerID == farmerId && c.CollectionDate >= from.Date && c.CollectionDate <= to.Date).ToList());

        public Task<bool> HasLockedInShiftAsync(int societyId, DateTime date, Shift shift, CancellationToken ct = default)
            => Task.FromResult(Items.Any(c => c.SocietyID == societyId && c.CollectionDate == date.Date && c.Shift == shift && c.IsLocked));
    }

    internal sealed class FakeMilkRateRepository : ListRepo<MilkRate>, IMilkRateRepository
    {
        public FakeMilkRateRepository(MemoryStore store) : base(store.Rates) { }
        protected override string IdName => nameof(MilkRate.RateID);

        public Task<List<MilkRate>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(r => r.SocietyID == societyId).ToList());

        public Task<bool> RangeOverlapsAsync(int societyId, decimal fatPercentFrom, decimal fatPercentTo, decimal snfPercentFrom, decimal snfPercentTo, decimal clrFrom, decimal clrTo, DateTime effectiveFrom, int? excludingRateId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<MilkRate?> GetApplicableRateAsync(int societyId, decimal fatPercent, decimal snf, decimal clr, DateTime collectionDate, CancellationToken ct = default)
            => Task.FromResult(MilkRateLookup.SelectApplicable(Items.Where(r => r.SocietyID == societyId), fatPercent, snf, clr, collectionDate));
    }

    internal sealed class FakeFeedInventoryRepository : ListRepo<FeedInventory>, IFeedInventoryRepository
    {
        public FakeFeedInventoryRepository(MemoryStore store) : base(store.Inventory) { }
        protected override string IdName => nameof(FeedInventory.FeedItemID);

        public Task<List<FeedInventory>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(i => i.SocietyID == societyId).ToList());

        public Task<FeedInventory?> GetByIdWithinSocietyAsync(int feedItemId, int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(i => i.FeedItemID == feedItemId && i.SocietyID == societyId));

        public Task<bool> NameExistsInSocietyAsync(string feedName, ItemType itemType, int societyId, int? excludingFeedItemId, CancellationToken ct = default)
            => Task.FromResult(Items.Any(i => i.SocietyID == societyId && i.ItemType == itemType && i.FeedName == feedName && i.FeedItemID != (excludingFeedItemId ?? 0)));
    }

    internal sealed class FakeFeedIssueRepository : ListRepo<FeedIssue>, IFeedIssueRepository
    {
        public FakeFeedIssueRepository(MemoryStore store) : base(store.Issues) { }
        protected override string IdName => nameof(FeedIssue.IssueID);

        public Task<List<FeedIssue>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(i => i.SocietyID == societyId).ToList());

        public Task<List<FeedIssue>> GetByFarmerAsync(int farmerId, int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(i => i.FarmerID == farmerId && i.SocietyID == societyId).ToList());

        public Task<List<FeedIssue>> GetUnlockedByFarmerAndPeriodAsync(int farmerId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
            => Task.FromResult(Items.Where(i => i.FarmerID == farmerId && !i.IsLocked && i.IssueDate >= periodStart.Date && i.IssueDate <= periodEnd.Date).ToList());

        public Task<List<FeedIssue>> GetLockedBySettlementAsync(int paymentId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(i => i.LockedBySettlementID == paymentId).ToList());

        public Task<List<FeedIssue>> GetBySocietyAndDateRangeAsync(int societyId, DateTime from, DateTime to, CancellationToken ct = default)
            => Task.FromResult(Items.Where(i => i.SocietyID == societyId && i.IssueDate >= from.Date && i.IssueDate <= to.Date).ToList());
    }

    internal sealed class FakePaymentRepository : ListRepo<Payment>, IPaymentRepository
    {
        public FakePaymentRepository(MemoryStore store) : base(store.Payments) { }
        protected override string IdName => nameof(Payment.PaymentID);

        public Task<List<Payment>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(p => p.SocietyID == societyId).ToList());

        public Task<Payment?> GetByIdWithinSocietyAsync(int paymentId, int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(p => p.PaymentID == paymentId && p.SocietyID == societyId));

        public Task<bool> ExistsNonCancelledForPeriodAsync(int farmerId, DateTime periodStart, int? excludingPaymentId, CancellationToken ct = default)
            => Task.FromResult(Items.Any(p => p.FarmerID == farmerId && p.PeriodStart == periodStart.Date && p.Status != SettlementStatus.Cancelled && p.PaymentID != (excludingPaymentId ?? 0)));

        public Task<Payment?> GetMostRecentBeforeAsync(int farmerId, DateTime periodStart, CancellationToken ct = default)
            => Task.FromResult(Items.Where(p => p.FarmerID == farmerId && p.PeriodStart < periodStart.Date && p.Status != SettlementStatus.Cancelled)
                .OrderByDescending(p => p.PeriodStart).FirstOrDefault());

        public Task<List<Payment>> GetGeneratedAcrossAllSocietiesAsync(CancellationToken ct = default)
            => Task.FromResult(Items.Where(p => p.Status == SettlementStatus.Generated).ToList());

        public Task<List<Payment>> GetByFarmerAsync(int farmerId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(p => p.FarmerID == farmerId).ToList());

        public Task<Payment?> GetByIdForFarmerAsync(int paymentId, int farmerId, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(p => p.PaymentID == paymentId && p.FarmerID == farmerId));
    }

    internal sealed class FakeDeductionRepository : ListRepo<SettlementDeduction>, ISettlementDeductionRepository
    {
        public FakeDeductionRepository(MemoryStore store) : base(store.Deductions) { }
        protected override string IdName => nameof(SettlementDeduction.SettlementDeductionID);
    }

    internal sealed class FakeAdvanceRepository : ListRepo<AdvancePayment>, IAdvancePaymentRepository
    {
        public FakeAdvanceRepository(MemoryStore store) : base(store.Advances) { }
        protected override string IdName => nameof(AdvancePayment.AdvancePaymentID);

        public Task<List<AdvancePayment>> GetUnappliedByFarmerAsync(int farmerId, DateTime? paidOnOrBefore = null, CancellationToken ct = default)
            => Task.FromResult(Items.Where(a => a.FarmerID == farmerId && !a.IsApplied && (paidOnOrBefore is null || a.PaymentDate <= paidOnOrBefore.Value.Date)).ToList());

        public Task<List<AdvancePayment>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(a => a.SocietyID == societyId).ToList());

        public Task<List<AdvancePayment>> GetAppliedToPaymentAsync(int paymentId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(a => a.AppliedToPaymentID == paymentId).ToList());
    }

    internal sealed class FakeDispatchRepository : ListRepo<Dispatch>, IDispatchRepository
    {
        public FakeDispatchRepository(MemoryStore store) : base(store.Dispatches) { }
        protected override string IdName => nameof(Dispatch.DispatchID);

        public Task<List<Dispatch>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(d => d.SocietyID == societyId).ToList());

        public Task<Dispatch?> GetByIdWithinSocietyAsync(int dispatchId, int societyId, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(d => d.DispatchID == dispatchId && d.SocietyID == societyId));

        public Task<bool> ExistsForDateAsync(int societyId, DateTime date, int? excludingDispatchId, CancellationToken ct = default)
            => Task.FromResult(Items.Any(d => d.SocietyID == societyId && d.DispatchDate == date.Date && d.DispatchID != (excludingDispatchId ?? 0)));

        public Task<List<Dispatch>> GetBySocietyAndDateRangeAsync(int societyId, DateTime from, DateTime to, CancellationToken ct = default)
            => Task.FromResult(Items.Where(d => d.SocietyID == societyId && d.DispatchDate >= from.Date && d.DispatchDate <= to.Date).ToList());
    }

    internal static class TestServices
    {
        public static IConfiguration DispatchConfig(decimal threshold = 5m) =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:DispatchVarianceThresholdPercent"] = threshold.ToString()
            }).Build();
    }
}
