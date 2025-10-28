using FitPass.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Common.Logging;

public partial class LogErrorMessages
{
    public static void FailedToRemoveUserFromTheirRole(ILogger logger, string userRole, string userId, Result result, Exception? exception)
    {
        _failedToRemoveUserFromTheirRole(logger, userRole, userId, result, exception);
    }

    public static void FailedToAddUserToRole(ILogger logger, string userId, string role, Result result, Exception? exception)
    {
        _failedToAddUserToRole(logger, userId, role, result, exception);
    }

    public static void UnhandledExceptionCaught(ILogger logger, string context, Exception exception)
    {
        _unhandledExceptionCaught(logger, context, exception);
    }
}
