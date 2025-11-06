using FitPass.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Common.Logging;

public partial class LogErrorMessages
{
    public static void IdentityServiceMethodFailed(ILogger logger, string methodName, IEnumerable<string>? userRole, string? userIdOrEmail, Result? result)
    {
        _identityServiceMethodFailed(logger, methodName, userRole, userIdOrEmail, result, null);
    }

    public static void UnhandledExceptionCaught(ILogger logger, string context, Exception exception)
    {
        _unhandledExceptionCaught(logger, context, exception);
    }

    public static void JsonSerilaizationFailure(ILogger logger, string serializationType, string methodName, string? ownerEntityName, string? ownerEntityId, string? jsonString, Exception? exception)
    {
        _jsonSerilaizationFailure(logger, serializationType, methodName, ownerEntityName, ownerEntityId, jsonString, exception);
    }
}
