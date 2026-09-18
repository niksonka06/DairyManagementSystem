namespace DairyManagementSystem.Helpers
{
    public static class DispatchVariance
    {
        public static decimal Percent(decimal totalCollected, decimal totalDispatched)
        {
            if (totalCollected == 0)
            {
                return totalDispatched == 0 ? 0 : 100m;
            }

            return Math.Abs(totalCollected - totalDispatched) / totalCollected * 100;
        }

        public static bool RequiresReason(decimal totalCollected, decimal totalDispatched, decimal thresholdPercent)
        {
            if (totalCollected <= 0 && totalDispatched > 0)
            {
                return true;
            }

            return Percent(totalCollected, totalDispatched) > thresholdPercent;
        }
    }
}
