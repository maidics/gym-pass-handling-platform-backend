using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Common.Logging;

public partial class LogCriticalMessages
{
    public static void FailedToFindGymEmployeeButHasGymEmployment(ILogger logger, IEnumerable<string>? gymEmployeeRoles, string userId, GymEmployment gymEmployment, Exception? exception)
    {
        _failedToFindGymEmployeeButOwnsGymEmployment(logger, gymEmployeeRoles, userId, gymEmployment, exception);
    }

    public static void AuthenticatedUserRelatedEntityNotFound(ILogger logger, IEnumerable<string>? userRoles, string? userId, string entityType)
    {
        _authenticatedUserRelatedEntityNotFound(logger, userRoles, userId, entityType, null);
    }

    public static void UserRequestedToUseAnAlreadyExpiredPass(ILogger logger, string? userId, string passId)
    {
        _userRequestedToUseAnAlreadyExpiredPass(logger, userId, passId, null);
    }
}
