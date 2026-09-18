using DairyManagementSystem.Helpers;

namespace DairyManagementSystem.Tests
{
    public class DispatchVarianceTests
    {
        [Fact]
        public void RequiresReason_when_nothing_was_collected_but_volume_was_dispatched()
        {
            Assert.True(DispatchVariance.RequiresReason(0, 100, thresholdPercent: 5));
            Assert.Equal(100m, DispatchVariance.Percent(0, 100));
        }

        [Fact]
        public void RequiresReason_when_percent_exceeds_threshold()
        {
            Assert.True(DispatchVariance.RequiresReason(100, 90, thresholdPercent: 5));
            Assert.False(DispatchVariance.RequiresReason(100, 96, thresholdPercent: 5));
        }
    }
}
