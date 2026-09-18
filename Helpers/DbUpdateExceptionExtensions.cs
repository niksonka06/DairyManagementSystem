using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Helpers
{
    public static class DbUpdateExceptionExtensions
    {
        public static bool IsUniqueConstraintViolation(this DbUpdateException exception)
        {
            for (var inner = exception.InnerException; inner is not null; inner = inner.InnerException)
            {
                if (inner is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
