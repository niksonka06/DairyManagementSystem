using DairyManagementSystem.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Tests
{
    public class DbUpdateExceptionExtensionsTests
    {
        [Fact]
        public void IsUniqueConstraintViolation_is_false_without_sql_unique_error()
        {
            var ex = new DbUpdateException("failed", new InvalidOperationException("not unique"));
            Assert.False(ex.IsUniqueConstraintViolation());
        }
    }
}
