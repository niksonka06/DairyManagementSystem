using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Interfaces
{
    public interface ISocietyRepository : IRepository<Society>
    {
        Task<bool> RegistrationNoExistsAsync(string registrationNo, int? excludingSocietyId, CancellationToken ct = default);
    }
}
