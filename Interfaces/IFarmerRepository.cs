using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Interfaces
{
    public interface IFarmerRepository : IRepository<Farmer>
    {
        Task<bool> FarmerCodeExistsInSocietyAsync(string farmerCode, int societyId, int? excludingFarmerId, CancellationToken ct = default);

        // Scoped list — an Operator only ever sees farmers in their own
        // society, enforced here (not just filtered client-side) so a
        // compromised/buggy view can't leak another society's farmers.
        Task<List<Farmer>> GetBySocietyAsync(int societyId, CancellationToken ct = default);

        Task<Farmer?> GetByIdWithinSocietyAsync(int farmerId, int societyId, CancellationToken ct = default);
    }
}
