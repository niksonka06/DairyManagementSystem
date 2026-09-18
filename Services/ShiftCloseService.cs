using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Services
{
    public class ShiftCloseService : IShiftCloseService
    {
        private readonly IShiftCloseRepository _shiftCloseRepository;
        private readonly IMilkCollectionRepository _collectionRepository;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public ShiftCloseService(
            IShiftCloseRepository shiftCloseRepository,
            IMilkCollectionRepository collectionRepository,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _shiftCloseRepository = shiftCloseRepository;
            _collectionRepository = collectionRepository;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> IsClosedAsync(int societyId, DateTime date, Shift shift, CancellationToken ct = default)
        {
            return await _shiftCloseRepository.GetAsync(societyId, date, shift, ct) is not null;
        }

        public async Task<IReadOnlyCollection<Shift>> GetClosedShiftsAsync(int societyId, DateTime date, CancellationToken ct = default)
        {
            var rows = await _shiftCloseRepository.GetBySocietyAndDateAsync(societyId, date, ct);
            return rows.Select(r => r.Shift).ToList();
        }

        public async Task CloseAsync(int societyId, DateTime date, Shift shift, int performedByUserId, CancellationToken ct = default)
        {
            var day = date.Date;
            if (await _shiftCloseRepository.GetAsync(societyId, day, shift, ct) is not null)
            {
                throw new BusinessRuleException($"The {shift} shift on {day:dd-MMM-yyyy} is already closed.");
            }

            var close = new ShiftClose
            {
                SocietyID = societyId,
                CollectionDate = day,
                Shift = shift,
                ClosedBy = performedByUserId,
                ClosedAt = DateTime.UtcNow
            };

            _shiftCloseRepository.Add(close);
            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                throw new BusinessRuleException($"The {shift} shift on {day:dd-MMM-yyyy} is already closed.");
            }

            _auditService.Log(nameof(ShiftClose), close.ShiftCloseID, AuditAction.ShiftClosed,
                oldValue: null,
                newValue: new { close.CollectionDate, close.Shift },
                performedByUserId,
                societyId);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task ReopenAsync(int societyId, DateTime date, Shift shift, int performedByUserId, CancellationToken ct = default)
        {
            var day = date.Date;
            var close = await _shiftCloseRepository.GetAsync(societyId, day, shift, ct)
                ?? throw new BusinessRuleException($"The {shift} shift on {day:dd-MMM-yyyy} is not closed.");

            if (await _collectionRepository.HasLockedInShiftAsync(societyId, day, shift, ct))
            {
                throw new BusinessRuleException(
                    $"Cannot reopen the {shift} shift on {day:dd-MMM-yyyy} because one or more collections are locked by a generated settlement.");
            }

            var id = close.ShiftCloseID;
            _shiftCloseRepository.Remove(close);
            await _unitOfWork.SaveChangesAsync(ct);

            _auditService.Log(nameof(ShiftClose), id, AuditAction.ShiftReopened,
                oldValue: new { day, shift },
                newValue: null,
                performedByUserId,
                societyId);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task<bool> CanReopenAsync(int societyId, DateTime date, Shift shift, CancellationToken ct = default)
        {
            if (!await IsClosedAsync(societyId, date, shift, ct))
            {
                return false;
            }

            return !await _collectionRepository.HasLockedInShiftAsync(societyId, date, shift, ct);
        }
    }
}
