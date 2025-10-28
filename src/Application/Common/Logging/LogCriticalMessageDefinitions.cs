using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Common.Logging;

public static partial class LogCriticalMessages
{
    private static readonly Action<ILogger, string, string, Exception?> _gymEmployeeGymEmploymentNotFound =
        LoggerMessage.Define<string, string>(
            LogLevel.Critical,
            eventId: new EventId(),
            formatString: "{GymEmployeeRole} user's ({UserId}) GymEmployment not found.");
    
    private static readonly Action<ILogger, string, string, GymEmployment, Exception?> _failedToFindGymEmployeeButOwnsGymEmployment =
        LoggerMessage.Define<string, string, GymEmployment>(
            LogLevel.Critical,
            eventId: new EventId(),
            formatString: "Failed to find {UserRole} user ({UserId}), but they own a GymEmployment entity: {GymEmployment}");
}
