namespace FitPass.Application.Common.Extensions;

public static class DateTimeOffsetExtensions
{
    public static bool IsToday(this DateTimeOffset dateTimeOffset)
    {
        var now = DateTimeOffset.UtcNow;

        return now.Date == dateTimeOffset.Date;
    }
}
