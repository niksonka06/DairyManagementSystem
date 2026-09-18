using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Helpers
{
    public static class StockConcurrency
    {
        public const string RetryMessage = "Stock was updated by someone else. Reload and try again.";

        public static async Task SaveOrThrowConcurrentAsync(Func<Task> save, CancellationToken ct = default)
        {
            try
            {
                await save();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessRuleException(RetryMessage);
            }
        }
    }
}
