using FitPass.Domain.Entities;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private readonly string _userGymMembership1Id = "UserGymMembership1";
    private readonly string _userGymMembershipId2 = "UserGymMembership2";
    public async Task SeedUserGymMembershipsAsync()
    {
        List<UserGymMembership> ugms = [
                new UserGymMembership {
                    Id = _userGymMembership1Id,
                    UserId = "User1",
                    GymId = gymId1
                },
                new UserGymMembership {
                    Id = _userGymMembershipId2,
                    UserId = "User1",
                    GymId = gymId2
                }
            ];

        await _context.AddRangeAsync(ugms);
    }
}
