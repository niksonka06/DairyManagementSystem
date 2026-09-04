using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Services
{
    public class FeedInventoryService : IFeedInventoryService
    {
        private readonly IFeedInventoryRepository _feedInventoryRepository;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public FeedInventoryService(IFeedInventoryRepository feedInventoryRepository, IAuditService auditService, IUnitOfWork unitOfWork)
        {
            _feedInventoryRepository = feedInventoryRepository;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<FeedInventory>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await _feedInventoryRepository.GetBySocietyAsync(societyId, ct);
        }

        public async Task<FeedInventory> CreateAsync(FeedInventoryFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            if (await _feedInventoryRepository.NameExistsInSocietyAsync(model.FeedName, model.ItemType, model.SocietyID, excludingFeedItemId: null, ct))
            {
                throw new BusinessRuleException($"A {model.ItemType} item named '{model.FeedName.Trim()}' already exists for this society.");
            }

            var item = new FeedInventory
            {
                SocietyID = model.SocietyID,
                ItemType = model.ItemType,
                FeedName = model.FeedName.Trim(),
                Unit = model.Unit.Trim(),
                PricePerUnit = model.PricePerUnit!.Value,
                LowStockThreshold = model.LowStockThreshold,
                StockQuantity = 0, // new items start empty — stock is added via AddStockAsync, its own audited workflow
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _feedInventoryRepository.Add(item);
            await _unitOfWork.SaveChangesAsync(ct);

            _auditService.Log(nameof(FeedInventory), item.FeedItemID, AuditAction.Created,
                oldValue: null,
                newValue: new { item.ItemType, item.FeedName, item.Unit, item.PricePerUnit },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);

            return item;
        }

        public async Task UpdateAsync(FeedInventoryFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var item = await _feedInventoryRepository.GetByIdWithinSocietyAsync(model.FeedItemID, model.SocietyID, ct)
                ?? throw new BusinessRuleException("Item not found.");

            if (await _feedInventoryRepository.NameExistsInSocietyAsync(model.FeedName, model.ItemType, model.SocietyID, excludingFeedItemId: item.FeedItemID, ct))
            {
                throw new BusinessRuleException($"A {model.ItemType} item named '{model.FeedName.Trim()}' already exists for this society.");
            }

            var oldSnapshot = new { item.ItemType, item.FeedName, item.Unit, item.PricePerUnit, item.LowStockThreshold };

            // Deliberately NOT touching item.StockQuantity here — this form
            // edits the item's master data (name, price, threshold), never
            // its stock level. Stock only ever changes via AddStockAsync
            // (top-up) or the atomic deduction in FeedIssueService — both
            // separately audited, so stock changes are always traceable to
            // a specific reason.
            item.ItemType = model.ItemType;
            item.FeedName = model.FeedName.Trim();
            item.Unit = model.Unit.Trim();
            item.PricePerUnit = model.PricePerUnit!.Value;
            item.LowStockThreshold = model.LowStockThreshold;

            _feedInventoryRepository.SetOriginalRowVersion(item, model.RowVersion!);

            _auditService.Log(nameof(FeedInventory), item.FeedItemID, AuditAction.Updated, oldSnapshot,
                new { item.ItemType, item.FeedName, item.Unit, item.PricePerUnit, item.LowStockThreshold },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task AddStockAsync(AddStockViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var item = await _feedInventoryRepository.GetByIdWithinSocietyAsync(model.FeedItemID, model.SocietyID, ct)
                ?? throw new BusinessRuleException("Item not found.");

            var oldQuantity = item.StockQuantity;
            item.StockQuantity += model.QuantityToAdd!.Value;

            _auditService.Log(nameof(FeedInventory), item.FeedItemID, AuditAction.Updated,
                oldValue: new { StockQuantity = oldQuantity },
                newValue: new { StockQuantity = item.StockQuantity, Added = model.QuantityToAdd!.Value },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task SetActiveStatusAsync(int feedItemId, int societyId, bool isActive, int performedByUserId, CancellationToken ct = default)
        {
            var item = await _feedInventoryRepository.GetByIdWithinSocietyAsync(feedItemId, societyId, ct)
                ?? throw new BusinessRuleException("Item not found.");

            if (item.IsActive == isActive)
            {
                return;
            }

            item.IsActive = isActive;

            _auditService.Log(nameof(FeedInventory), item.FeedItemID,
                isActive ? AuditAction.Activated : AuditAction.Deactivated,
                oldValue: new { IsActive = !isActive },
                newValue: new { IsActive = isActive },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
