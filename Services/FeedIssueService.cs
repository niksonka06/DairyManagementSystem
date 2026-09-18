using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Services
{
    public class FeedIssueService : IFeedIssueService
    {
        private readonly IFeedIssueRepository _feedIssueRepository;
        private readonly IFeedInventoryRepository _feedInventoryRepository;
        private readonly IFarmerRepository _farmerRepository;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public FeedIssueService(
            IFeedIssueRepository feedIssueRepository,
            IFeedInventoryRepository feedInventoryRepository,
            IFarmerRepository farmerRepository,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _feedIssueRepository = feedIssueRepository;
            _feedInventoryRepository = feedInventoryRepository;
            _farmerRepository = farmerRepository;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<FeedIssue>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await _feedIssueRepository.GetBySocietyAsync(societyId, ct);
        }

        public async Task<List<FeedIssue>> GetByFarmerAsync(int farmerId, int societyId, CancellationToken ct = default)
        {
            return await _feedIssueRepository.GetByFarmerAsync(farmerId, societyId, ct);
        }

        public async Task<FeedIssue> IssueToFarmerAsync(FeedIssueFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            // Ownership checks — never trust that a posted FeedItemID/FarmerID
            // actually belongs to this Operator's own society.
            var item = await _feedInventoryRepository.GetByIdWithinSocietyAsync(model.FeedItemID, model.SocietyID, ct)
                ?? throw new BusinessRuleException("Item not found in this society.");

            if (!item.IsActive)
            {
                throw new BusinessRuleException($"'{item.FeedName}' is inactive and cannot be issued.");
            }

            var farmer = await _farmerRepository.GetByIdAsync(model.FarmerID, ct);
            if (farmer is null || farmer.SocietyID != model.SocietyID)
            {
                throw new BusinessRuleException("Selected farmer does not belong to this society.");
            }

            if (!farmer.IsActive)
            {
                throw new BusinessRuleException("Selected farmer is inactive and cannot receive feed or medicine issues.");
            }

            // ── The synopsis's exact atomic sequence ──────────────────────
            // Check stock -> Deduct stock -> Create farmer issue record ->
            // Create deduction -> Commit transaction. If anything fails,
            // rollback everything.
            //
            // Unlike Farmer creation (Stage 5), this doesn't need an EXPLICIT
            // BeginTransactionAsync/Commit — everything here (the stock
            // deduction and the issue record) is a plain EF Core entity
            // change on the SAME DbContext. A single SaveChangesAsync() call
            // already wraps both changes in one database transaction — EF
            // Core does this automatically. Explicit transaction control
            // (like Stage 5 used) is only needed when mixing with something
            // that commits independently, like Identity's UserManager —
            // not the case here.

            var quantity = model.Quantity!.Value;

            // 1. CHECK STOCK
            if (item.StockQuantity < quantity)
            {
                throw new BusinessRuleException(
                    $"Insufficient stock for '{item.FeedName}'. Available: {item.StockQuantity} {item.Unit}, requested: {quantity} {item.Unit}.");
            }

            // 2. DEDUCT STOCK
            item.StockQuantity -= quantity;
            _feedInventoryRepository.SetOriginalRowVersion(item, item.RowVersion);

            var issue = new FeedIssue
            {
                FeedItemID = item.FeedItemID,
                FarmerID = farmer.FarmerID,
                SocietyID = model.SocietyID,
                ItemType = item.ItemType,
                Quantity = quantity,
                UnitPriceAtIssue = item.PricePerUnit, // snapshot — see FeedIssue class comment
                TotalCost = quantity * item.PricePerUnit,
                IssueDate = model.IssueDate.Date,
                IssuedBy = performedByUserId,
                CreatedAt = DateTime.UtcNow,
                IsLocked = false
            };

            _feedIssueRepository.Add(issue);

            // 5. COMMIT TRANSACTION — one SaveChangesAsync persists both the
            // stock deduction on `item` and the new `issue` row together. If
            // this throws for any reason, NEITHER change is persisted —
            // EF Core's own transaction guarantees that.
            await StockConcurrency.SaveOrThrowConcurrentAsync(() => _unitOfWork.SaveChangesAsync(ct), ct);

            _auditService.Log(nameof(FeedIssue), issue.IssueID, AuditAction.Created,
                oldValue: null,
                newValue: new { issue.FarmerID, issue.FeedItemID, issue.ItemType, issue.Quantity, issue.TotalCost, issue.IssueDate },
                performedByUserId, issue.SocietyID);

            await _unitOfWork.SaveChangesAsync(ct);

            return issue;
        }

        public async Task VoidUnlockedAsync(int issueId, int societyId, int performedByUserId, CancellationToken ct = default)
        {
            var issue = await _feedIssueRepository.GetByIdAsync(issueId, ct)
                ?? throw new BusinessRuleException("Issue not found.");

            if (issue.SocietyID != societyId)
            {
                throw new BusinessRuleException("Issue not found in this society.");
            }

            if (issue.IsLocked)
            {
                throw new BusinessRuleException("This issue is locked in a generated settlement and cannot be voided. An Admin must unlock the settlement first.");
            }

            var item = await _feedInventoryRepository.GetByIdWithinSocietyAsync(issue.FeedItemID, societyId, ct)
                ?? throw new BusinessRuleException("The inventory item for this issue is missing.");

            var oldStock = item.StockQuantity;
            item.StockQuantity += issue.Quantity;
            _feedInventoryRepository.SetOriginalRowVersion(item, item.RowVersion);

            _auditService.Log(nameof(FeedIssue), issue.IssueID, AuditAction.Deleted,
                oldValue: new { issue.FarmerID, issue.FeedItemID, issue.Quantity, issue.TotalCost, issue.IssueDate },
                newValue: new { Voided = true, StockRestoredTo = item.StockQuantity, StockWas = oldStock },
                performedByUserId, issue.SocietyID);

            _feedIssueRepository.Remove(issue);

            await StockConcurrency.SaveOrThrowConcurrentAsync(() => _unitOfWork.SaveChangesAsync(ct), ct);
        }
    }
}
