using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Common.Logging;

public partial class LogCriticalMessages
{
    public static void AuthenticatedGymEmployeeGymEmploymentNotFound(ILogger logger, IEnumerable<string>? gymEmployeeRoles, string? gymEmployeeUserId, Exception? exception)
    {
        _authenticatedGymEmployeeGymEmploymentNotFound(logger, gymEmployeeRole, gymEmployeeUserId, exception);
    }
    public static void FailedToFindGymEmployeeButHasGymEmployment(ILogger logger, string gymEmployeeRole, string userId, GymEmployment gymEmployment, Exception? exception)
    {
        _failedToFindGymEmployeeButOwnsGymEmployment(logger, gymEmployeeRole, userId, gymEmployment, exception);
    }

    public static void AuthenticatedUserNotFound(ILogger logger, IEnumerable<string>? userRole, string? userId, Exception? exception)
    {
        _authenticatedUserNotFound(logger, userRole, userId, exception);
    }
}
