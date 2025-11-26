namespace FitPass.Domain.Enums;

public static class DayOfWeekExtensions
{
    extension(DayOfWeek day)
    {
        public string ToLowerCaseString()
        {
            return day switch
            {
                DayOfWeek.Monday => "monday",
                DayOfWeek.Tuesday => "tuesday",
                DayOfWeek.Wednesday => "wednesday",
                DayOfWeek.Thursday => "thursday",
                DayOfWeek.Friday => "friday",
                DayOfWeek.Saturday => "saturday",
                DayOfWeek.Sunday => "sunday",
                _ => throw new ArgumentException($"Parameter: '{day}' is not a day of week.")
            };
        }
    }
}
