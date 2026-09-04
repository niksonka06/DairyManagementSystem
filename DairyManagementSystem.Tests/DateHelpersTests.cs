using DairyManagementSystem.Helpers;

namespace DairyManagementSystem.Tests
{
    public class DateHelpersTests
    {
        [Theory]
        [InlineData("2026-08-31", "2026-08-31", "2026-09-06")] // Monday
        [InlineData("2026-09-02", "2026-08-31", "2026-09-06")] // Wednesday
        [InlineData("2026-09-06", "2026-08-31", "2026-09-06")] // Sunday
        public void ComputeWeek_is_monday_through_sunday(string reference, string start, string end)
        {
            var (weekStart, weekEnd) = DateHelpers.ComputeWeek(DateTime.Parse(reference));
            Assert.Equal(DateTime.Parse(start), weekStart);
            Assert.Equal(DateTime.Parse(end), weekEnd);
        }
    }
}
