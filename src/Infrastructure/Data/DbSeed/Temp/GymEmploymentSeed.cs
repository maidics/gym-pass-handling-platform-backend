using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private async Task SeedGymEmploymentsAsync()
    {
        var now = DateTimeOffset.UtcNow;

        var gymEmployments = new List<GymEmployment>()
        {
            new GymEmployment
            {
                GymId = "TestGymId",
                UserId = "GymAdminLocalhostId",
                Role = Roles.GymAdministrator,
                SupervisorEmail = "escalation@localhost"
            },
            new GymEmployment
            {
                GymId = "TestGymId",
                UserId = "GymStaffLocalhostId",
                Role = Roles.GymStaff,
                SupervisorEmail = "escalation@localhost"
            }
        };

        await _context.GymEmployments.AddRangeAsync(gymEmployments);
    }
}
