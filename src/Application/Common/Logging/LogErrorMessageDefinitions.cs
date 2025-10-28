using FitPass.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Common.Logging;

public partial class LogErrorMessages
{
    private static readonly Action<ILogger, string, string, Result, Exception?> _failedToRemoveUserFromTheirRole =
        LoggerMessage.Define<string, string, Result>(
            logLevel: LogLevel.Error,
            eventId: new EventId(),
            formatString: "Failed to remove {UserRole} user ({UserId}) from their role. Result: {Result}");

    private static readonly Action<ILogger, string, string, Result, Exception?> _failedToAddUserToRole =
        LoggerMessage.Define<string, string, Result>(
            logLevel: LogLevel.Error,
            eventId: new EventId(),
            formatString: "Failed to add User ({UserId}) to {Role} role. Result: {Result}");

    private static readonly Action<ILogger, string, Exception> _unhandledExceptionCaught =
        LoggerMessage.Define<string>(
            logLevel: LogLevel.Error,
            eventId: new EventId(),
            formatString: "Unhandled error caught in {Context}.");

    private static readonly Action<ILogger, string, Result, Exception?> _registrationFailed =
        LoggerMessage.Define<string, Result>(
            logLevel: LogLevel.Error,
            eventId: new EventId(),
            formatString: "{UserRole} user registration failed. Result {Result}");
}
