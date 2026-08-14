namespace DairyManagementSystem.Helpers
{
    public static class DateHelpers
    {
        // .NET's DayOfWeek: Sunday=0 ... Saturday=6. We want Monday-start
        // weeks, so shift so Monday=0. Used by both Settlement generation
        // and the Weekly Collection Summary report — same week definition.
        public static (DateTime Start, DateTime End) ComputeWeek(DateTime referenceDate)
        {
            var day = referenceDate.Date;
            var offset = ((int)day.DayOfWeek + 6) % 7;
            var start = day.AddDays(-offset);
            var end = start.AddDays(6);
            return (start, end);
        }
    }
}
