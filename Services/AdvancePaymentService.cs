using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Services
{
    public class AdvancePaymentService : IAdvancePaymentService
    {
        private readonly IAdvancePaymentRepository _advancePaymentRepository;
        private readonly IFarmerRepository _farmerRepository;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public AdvancePaymentService(
            IAdvancePaymentRepository advancePaymentRepository,
            IFarmerRepository farmerRepository,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _advancePaymentRepository = advancePaymentRepository;
            _farmerRepository = farmerRepository;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AdvancePayment>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await _advancePaymentRepository.GetBySocietyAsync(societyId, ct);
        }

        public async Task<AdvancePayment> RecordAsync(AdvancePaymentFormViewModel model, int performedByUserId, CancellationToken ct = default)
        {
            var farmer = await _farmerRepository.GetByIdAsync(model.FarmerID, ct);
            if (farmer is null || farmer.SocietyID != model.SocietyID)
            {
                throw new BusinessRuleException("Selected farmer does not belong to this society.");
            }

            if (!farmer.IsActive)
            {
                throw new BusinessRuleException("Selected farmer is inactive and cannot receive advance payments.");
            }

            var advance = new AdvancePayment
            {
                FarmerID = model.FarmerID,
                SocietyID = model.SocietyID,
                Amount = model.Amount,
                PaymentDate = model.PaymentDate.Date,
                RecordedBy = performedByUserId,
                CreatedAt = DateTime.UtcNow,
                IsApplied = false
            };

            _advancePaymentRepository.Add(advance);
            await _unitOfWork.SaveChangesAsync(ct);

            _auditService.Log(nameof(AdvancePayment), advance.AdvancePaymentID, AuditAction.Created,
                oldValue: null,
                newValue: new { advance.FarmerID, advance.Amount, advance.PaymentDate },
                performedByUserId);

            await _unitOfWork.SaveChangesAsync(ct);

            return advance;
        }
    }
}
