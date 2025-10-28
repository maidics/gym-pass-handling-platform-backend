using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Common.Logging;

public static partial class LogCriticalMessages
{
    private static readonly Action<ILogger, IEnumerable<string>?, string?, Exception?> _authenticatedGymEmployeeGymEmploymentNotFound =
        LoggerMessage.Define<IEnumerable<string>?, string?>(
            LogLevel.Critical,
            eventId: new EventId(),
            formatString: "Authenticated {GymEmployeeRoles} user's ({UserId}) GymEmployment not found.");

    private static readonly Action<ILogger, IEnumerable<string>?, string, GymEmployment, Exception?> _failedToFindGymEmployeeButOwnsGymEmployment =
        LoggerMessage.Define<IEnumerable<string>?, string, GymEmployment>(
            LogLevel.Critical,
            eventId: new EventId(),
            formatString: "Failed to find {GymEmployeeRoles} user ({UserId}), but they own a GymEmployment entity: {GymEmployment}");

    private static readonly Action<ILogger, IEnumerable<string>?, string?, Exception?> _authenticatedUserNotFound =
        LoggerMessage.Define<IEnumerable<string>?, string?>(
            LogLevel.Critical,
            eventId: new EventId(),
            formatString: "Authenticated {UserRoles} user ({UserId}) not found.");
}
