using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Repositories
{
    public class SettlementDeductionRepository : RepositoryBase<SettlementDeduction>, ISettlementDeductionRepository
    {
        public SettlementDeductionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
