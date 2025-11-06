using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Common.Logging;

public static partial class LogCriticalMessages
{
    private static readonly Action<ILogger, IEnumerable<string>?, string, GymEmployment, Exception?> _failedToFindGymEmployeeButOwnsGymEmployment =
        LoggerMessage.Define<IEnumerable<string>?, string, GymEmployment>(
            LogLevel.Critical,
            eventId: new EventId(),
            formatString: "Failed to find {GymEmployeeRoles} user ({UserId}), but they own a GymEmployment entity: {GymEmployment}");

    private static readonly Action<ILogger, IEnumerable<string>?, string?, string, Exception?> _authenticatedUserRelatedEntityNotFound =
        LoggerMessage.Define<IEnumerable<string>?, string?, string>(
            LogLevel.Critical,
            eventId: new EventId(),
            formatString: "Authenticated {UserRoles} user's ({UserId}) {EntityType} entity not found.");
}
