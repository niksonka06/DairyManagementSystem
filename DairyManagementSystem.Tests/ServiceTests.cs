using System.ComponentModel.DataAnnotations;
using DairyManagementSystem.Helpers;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using DairyManagementSystem.Services;

namespace DairyManagementSystem.Tests
{
    public class CollectionValidationTests
    {
        [Theory]
        [InlineData(0.4, 4, 8.5, 28, false)]
        [InlineData(501, 4, 8.5, 28, false)]
        [InlineData(5, 2.4, 8.5, 28, false)]
        [InlineData(5, 9.1, 8.5, 28, false)]
        [InlineData(5, 4, 7.4, 28, false)]
        [InlineData(5, 4, 11.1, 28, false)]
        [InlineData(5, 4, 8.5, 28, true)]
        public void Collection_form_enforces_synopsis_ranges(decimal qty, decimal fat, decimal snf, decimal clr, bool expectedValid)
        {
            var model = new MilkCollectionFormViewModel
            {
                FarmerID = 1,
                CollectionDate = DateTime.Today,
                Shift = Shift.Morning,
                Quantity = qty,
                FatPercent = fat,
                SNF = snf,
                CLR = clr
            };

            var results = new List<ValidationResult>();
            var ok = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
            Assert.Equal(expectedValid, ok);
        }
    }

    public class MilkRateServiceTests
    {
        [Fact]
        public async Task GetApplicableRate_uses_latest_chart_and_ignores_inactive()
        {
            var store = new MemoryStore();
            store.Rates.AddRange(new[]
            {
                Band(1, new DateTime(2026, 1, 1), 3, 6, 30),
                Band(2, new DateTime(2026, 6, 1), 4.6m, 6, 45),
                Band(3, new DateTime(2026, 6, 1), 3, 6, 99, active: false)
            });
            var service = new MilkRateService(new FakeMilkRateRepository(store), new FakeAuditService(), new FakeUnitOfWork(store));

            var match = await service.GetApplicableRateAsync(1, 5.0m, 8.5m, 28m, new DateTime(2026, 8, 15));
            Assert.Equal(45m, match!.RatePerLitre);

            var none = await service.GetApplicableRateAsync(1, 8.0m, 8.5m, 28m, new DateTime(2026, 8, 15));
            Assert.Null(none);
        }

        private static MilkRate Band(int id, DateTime from, decimal fatFrom, decimal fatTo, decimal rate, bool active = true) => new()
        {
            RateID = id,
            SocietyID = 1,
            FatPercentFrom = fatFrom,
            FatPercentTo = fatTo,
            SnfPercentFrom = 7.5m,
            SnfPercentTo = 11m,
            ClrFrom = 0,
            ClrTo = 50,
            RatePerLitre = rate,
            EffectiveFrom = from,
            IsActive = active
        };
    }

    public class MilkCollectionServiceTests
    {
        private static (MilkCollectionService Svc, MemoryStore Store, FakeShiftCloseService Shifts) Create()
        {
            var store = new MemoryStore();
            store.Farmers.Add(new Farmer { FarmerID = 1, SocietyID = 1, FullName = "A", FarmerCode = "F001", IsActive = true });
            store.Rates.Add(new MilkRate
            {
                RateID = 1, SocietyID = 1, FatPercentFrom = 3, FatPercentTo = 6,
                SnfPercentFrom = 7.5m, SnfPercentTo = 11, ClrFrom = 0, ClrTo = 50,
                RatePerLitre = 40, EffectiveFrom = new DateTime(2020, 1, 1), IsActive = true
            });
            var shifts = new FakeShiftCloseService();
            var svc = new MilkCollectionService(
                new FakeCollectionRepository(store),
                new FakeFarmerRepository(store),
                new MilkRateService(new FakeMilkRateRepository(store), new FakeAuditService(), new FakeUnitOfWork(store)),
                shifts,
                new FakeAuditService(),
                new FakeUnitOfWork(store));
            return (svc, store, shifts);
        }

        private static MilkCollectionFormViewModel Form(decimal qty = 5, decimal fat = 4.2m) => new()
        {
            FarmerID = 1,
            SocietyID = 1,
            CollectionDate = DateTime.Today,
            Shift = Shift.Morning,
            Quantity = qty,
            FatPercent = fat,
            SNF = 8.5m,
            CLR = 28m
        };

        [Fact]
        public async Task Create_snapshots_qty_times_rate()
        {
            var (svc, _, _) = Create();
            var created = await svc.CreateAsync(Form(5), 9);
            Assert.Equal(40m, created.RatePerLitre);
            Assert.Equal(200m, created.Amount);
        }

        [Fact]
        public async Task Create_rejects_duplicate_farmer_date_shift()
        {
            var (svc, _, _) = Create();
            await svc.CreateAsync(Form(), 9);
            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => svc.CreateAsync(Form(), 9));
            Assert.Contains("already exists", ex.Message);
        }

        [Fact]
        public async Task Update_blocked_when_locked()
        {
            var (svc, store, _) = Create();
            var created = await svc.CreateAsync(Form(), 9);
            created.IsLocked = true;
            created.RowVersion = new byte[] { 1 };
            var form = Form(6);
            form.CollectionID = created.CollectionID;
            form.RowVersion = created.RowVersion;
            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => svc.UpdateAsync(form, 9));
            Assert.Contains("locked", ex.Message);
        }

        [Fact]
        public async Task Create_blocked_when_shift_closed()
        {
            var (svc, _, shifts) = Create();
            shifts.Closed.Add((1, DateTime.Today, Shift.Morning));
            await Assert.ThrowsAsync<BusinessRuleException>(() => svc.CreateAsync(Form(), 9));
        }

        [Fact]
        public async Task Farmer_cannot_load_another_farmers_collection()
        {
            var (svc, store, _) = Create();
            store.Farmers.Add(new Farmer { FarmerID = 2, SocietyID = 1, FullName = "B", FarmerCode = "F002", IsActive = true });
            var created = await svc.CreateAsync(Form(), 9);
            var leaked = await svc.GetByIdForFarmerAsync(created.CollectionID, farmerId: 2);
            Assert.Null(leaked);
            var own = await svc.GetByIdForFarmerAsync(created.CollectionID, farmerId: 1);
            Assert.NotNull(own);
        }
    }

    public class PaymentServiceTests
    {
        [Fact]
        public async Task Draft_uses_synopsis_net_formula_then_generate_locks_and_duplicate_is_blocked()
        {
            var store = new MemoryStore();
            var (start, end) = DateHelpers.ComputeWeek(DateTime.Today);
            store.Farmers.Add(new Farmer { FarmerID = 1, SocietyID = 1, Phone = "999", IsActive = true });
            store.Collections.Add(new MilkCollection
            {
                CollectionID = 1, FarmerID = 1, SocietyID = 1, CollectionDate = start,
                Shift = Shift.Morning, Quantity = 10, RatePerLitre = 40, Amount = 400, IsLocked = false
            });
            store.Issues.Add(new FeedIssue
            {
                IssueID = 1, FarmerID = 1, SocietyID = 1, ItemType = ItemType.Feed,
                Quantity = 1, UnitPriceAtIssue = 50, TotalCost = 50, IssueDate = start, IsLocked = false
            });
            store.Issues.Add(new FeedIssue
            {
                IssueID = 2, FarmerID = 1, SocietyID = 1, ItemType = ItemType.Medicine,
                Quantity = 1, UnitPriceAtIssue = 20, TotalCost = 20, IssueDate = start, IsLocked = false
            });

            var payments = new FakePaymentRepository(store);
            var svc = new PaymentService(
                payments,
                new FakeDeductionRepository(store),
                new FakeAdvanceRepository(store),
                new FakeCollectionRepository(store),
                new FakeFeedIssueRepository(store),
                new FakeFarmerRepository(store),
                new FakeAuditService(),
                new FakeUnitOfWork(store),
                new FakeSmsService());

            var draft = await svc.CreateDraftAsync(new SettlementCreateViewModel
            {
                FarmerID = 1,
                SocietyID = 1,
                WeekReferenceDate = DateTime.Today,
                LoanDeduction = 10,
                PreviousDue = 0
            }, 9);

            Assert.Equal(400m, draft.GrossAmount);
            Assert.Equal(50m, draft.FeedDeduction);
            Assert.Equal(20m, draft.MedicineDeduction);
            Assert.Equal(10m, draft.OtherDeductionsTotal);
            Assert.Equal(320m, draft.NetAmount);

            await svc.GenerateAsync(draft.PaymentID, 1, 9, new byte[] { 1 });
            Assert.Equal(SettlementStatus.Generated, draft.Status);
            Assert.True(store.Collections[0].IsLocked);
            Assert.True(store.Issues[0].IsLocked);

            var dup = await Assert.ThrowsAsync<BusinessRuleException>(() => svc.CreateDraftAsync(new SettlementCreateViewModel
            {
                FarmerID = 1,
                SocietyID = 1,
                WeekReferenceDate = DateTime.Today
            }, 9));
            Assert.Contains("already exists", dup.Message);

            await svc.CancelGeneratedAsync(draft.PaymentID, 9, "Correction after fat dispute", new byte[] { 1 });
            Assert.Equal(SettlementStatus.Cancelled, draft.Status);
            Assert.False(store.Collections[0].IsLocked);

            Assert.Null(await svc.GetByIdForFarmerAsync(draft.PaymentID, farmerId: 99));
            Assert.NotNull(await svc.GetByIdForFarmerAsync(draft.PaymentID, farmerId: 1));
        }

        [Fact]
        public async Task Generated_settlement_can_be_marked_paid()
        {
            var store = new MemoryStore();
            var (start, _) = DateHelpers.ComputeWeek(DateTime.Today);
            store.Farmers.Add(new Farmer { FarmerID = 1, SocietyID = 1, Phone = "999", IsActive = true });
            store.Collections.Add(new MilkCollection
            {
                CollectionID = 1, FarmerID = 1, SocietyID = 1, CollectionDate = start,
                Shift = Shift.Morning, Quantity = 10, RatePerLitre = 40, Amount = 400, IsLocked = false
            });
            var svc = new PaymentService(
                new FakePaymentRepository(store),
                new FakeDeductionRepository(store),
                new FakeAdvanceRepository(store),
                new FakeCollectionRepository(store),
                new FakeFeedIssueRepository(store),
                new FakeFarmerRepository(store),
                new FakeAuditService(),
                new FakeUnitOfWork(store),
                new FakeSmsService());

            var draft = await svc.CreateDraftAsync(new SettlementCreateViewModel
            {
                FarmerID = 1,
                SocietyID = 1,
                WeekReferenceDate = DateTime.Today,
                PreviousDue = 0
            }, 9);
            await svc.GenerateAsync(draft.PaymentID, 1, 9, new byte[] { 1 });
            await svc.MarkPaidAsync(draft.PaymentID, 1, 9, new byte[] { 1 });
            Assert.Equal(SettlementStatus.Paid, draft.Status);
        }
    }

    public class DispatchServiceTests
    {
        [Fact]
        public async Task Create_requires_reason_when_variance_exceeds_threshold()
        {
            var store = new MemoryStore();
            store.Collections.Add(new MilkCollection
            {
                CollectionID = 1, SocietyID = 1, CollectionDate = DateTime.Today,
                Quantity = 100, Amount = 1, RatePerLitre = 1, FarmerID = 1, Shift = Shift.Morning
            });
            var svc = new DispatchService(
                new FakeDispatchRepository(store),
                new FakeCollectionRepository(store),
                new FakeAuditService(),
                new FakeUnitOfWork(store),
                TestServices.DispatchConfig(5));

            var form = new DispatchFormViewModel
            {
                SocietyID = 1,
                DispatchDate = DateTime.Today,
                DispatchTime = TimeSpan.FromHours(8),
                VehicleNo = "KL-01",
                Destination = "Union",
                TotalDispatched = 90
            };

            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => svc.CreateAsync(form, 9));
            Assert.Contains("reason is required", ex.Message);

            form.VarianceReason = "Weighbridge difference";
            var saved = await svc.CreateAsync(form, 9);
            Assert.Equal(100m, saved.TotalCollected);
            Assert.Equal(10m, saved.Variance);
        }
    }

    public class FeedIssueServiceTests
    {
        [Fact]
        public async Task Issue_decrements_stock_and_blocks_overdraw_and_concurrency()
        {
            var store = new MemoryStore();
            store.Farmers.Add(new Farmer { FarmerID = 1, SocietyID = 1, IsActive = true });
            store.Inventory.Add(new FeedInventory
            {
                FeedItemID = 1, SocietyID = 1, FeedName = "Cattle feed", Unit = "kg",
                PricePerUnit = 20, StockQuantity = 10, IsActive = true, ItemType = ItemType.Feed,
                RowVersion = new byte[] { 1 }
            });
            var uow = new FakeUnitOfWork(store);
            var svc = new FeedIssueService(
                new FakeFeedIssueRepository(store),
                new FakeFeedInventoryRepository(store),
                new FakeFarmerRepository(store),
                new FakeAuditService(),
                uow);

            var form = new FeedIssueFormViewModel { FeedItemID = 1, FarmerID = 1, SocietyID = 1, Quantity = 4, IssueDate = DateTime.Today };
            var issue = await svc.IssueToFarmerAsync(form, 9);
            Assert.Equal(6m, store.Inventory[0].StockQuantity);
            Assert.Equal(80m, issue.TotalCost);

            var over = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                svc.IssueToFarmerAsync(new FeedIssueFormViewModel { FeedItemID = 1, FarmerID = 1, SocietyID = 1, Quantity = 9, IssueDate = DateTime.Today }, 9));
            Assert.Contains("Insufficient stock", over.Message);

            uow.ThrowConcurrency = true;
            var conflict = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                svc.IssueToFarmerAsync(new FeedIssueFormViewModel { FeedItemID = 1, FarmerID = 1, SocietyID = 1, Quantity = 1, IssueDate = DateTime.Today }, 9));
            Assert.Equal(StockConcurrency.RetryMessage, conflict.Message);
        }
    }
}
