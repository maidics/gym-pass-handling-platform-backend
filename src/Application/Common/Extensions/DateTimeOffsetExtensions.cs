namespace FitPass.Application.Common.Extensions;

public static class DateTimeOffsetExtensions
{
    public static bool IsToday(this DateTimeOffset dateTimeOffset, DateTimeOffset utcNow)
    {
        return utcNow.UtcDateTime.Date == dateTimeOffset.UtcDateTime.Date;
    }
}
