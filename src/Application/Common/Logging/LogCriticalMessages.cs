using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Common.Logging;

public partial class LogCriticalMessages
{
    public static void GymEmployeeGymEmploymentNotFound(ILogger logger, string gymEmployeeRole, string userId, Exception? exception)
    {
        _gymEmployeeGymEmploymentNotFound(logger, gymEmployeeRole, userId, exception);
    }

    public static void FailedToFindGymEmployeeButHasGymEmployment(ILogger logger, string gymEmployeeRole, string userId, GymEmployment gymEmployment, Exception? exception)
    {
        _failedToFindGymEmployeeButOwnsGymEmployment(logger, gymEmployeeRole, userId, gymEmployment, exception);
    }
}
