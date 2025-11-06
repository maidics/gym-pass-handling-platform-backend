using FitPass.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Common.Logging;

public partial class LogErrorMessages
{
    private static readonly Action<ILogger, string, Exception> _unhandledExceptionCaught =
        LoggerMessage.Define<string>(
            logLevel: LogLevel.Error,
            eventId: new EventId(),
            formatString: "Unhandled error caught in {Context}.");

    private static readonly Action<ILogger, string, IEnumerable<string>?, string?, Result?, Exception?> _identityServiceMethodFailed =
        LoggerMessage.Define<string, IEnumerable<string>?, string?, Result?>(
            logLevel: LogLevel.Error,
            eventId: new EventId(),
            formatString: "IdentityService {MethodName} method failed for {UserRoles} user ({UserIdOrEmail}). Result: {Result}");

    private static readonly Action<ILogger, string, string, string?, string?, string?, Exception?> _jsonSerilaizationFailure =
        LoggerMessage.Define<string, string, string?, string?, string?>(
            logLevel: LogLevel.Error,
            eventId: new EventId(),
            formatString: "JSON {SerializationType} failed in {MethodName}, Owner entity: {OwnerEntityType} ({OwnerEntityId}), Json: {JsonString}");
}
