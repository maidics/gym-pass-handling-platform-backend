namespace FitPass.Application.Common.Extensions;

public static class DateTimeOffsetExtensions
{
    public static bool IsToday(this DateTimeOffset dateTimeOffset, DateTimeOffset utcNow)
    {
        return utcNow.ToUniversalTime().Date == dateTimeOffset.ToUniversalTime().Date;
    }
}
