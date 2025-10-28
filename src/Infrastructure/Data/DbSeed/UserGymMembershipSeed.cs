using FitPass.Domain.Entities;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private readonly string _userGymMembership1Id = "UserGymMembership1";
    private readonly string _userGymMembershipId2 = "UserGymMembership2";
    public async Task SeedUserGymMembershipsAsync()
    {
        List<GymMembership> ugms = [
                new GymMembership {
                    Id = _userGymMembership1Id,
                    ApplicationUserId = "User1",
                    NonRegisteredUserId = null,
                    GymId = gymId1
                },
                new GymMembership {
                    Id = _userGymMembershipId2,
                    ApplicationUserId = "User1",
                    NonRegisteredUserId = null,
                    GymId = gymId2
                }
            ];

        await _context.AddRangeAsync(ugms);
    }
}
