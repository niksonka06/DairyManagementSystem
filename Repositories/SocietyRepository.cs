using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    public class SocietyRepository : RepositoryBase<Society>, ISocietyRepository
    {
        public SocietyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> RegistrationNoExistsAsync(string registrationNo, int? excludingSocietyId, CancellationToken ct = default)
        {
            var normalized = registrationNo.Trim();
            return await DbSet.AnyAsync(
                s => s.RegistrationNo == normalized && s.SocietyID != (excludingSocietyId ?? 0), ct);
        }
    }
}
