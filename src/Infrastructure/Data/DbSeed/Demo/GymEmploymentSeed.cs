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
                GymId = "DemoGymId",
                UserId = "GymAdminLocalhostId",
                Role = Roles.GymAdministrator,
                SupervisorEmail = "supervisor@localhost.com",
                CreatedOn = now,
            },
            new GymEmployment
            {
                GymId = "DemoGymId",
                UserId = "GymStaffLocalhostId",
                Role = Roles.GymStaff,
                SupervisorEmail = "supervisor@localhost.com",
                CreatedOn = now,
            },
        };

        await _context.GymEmployments.AddRangeAsync(gymEmployments);
    }
}
