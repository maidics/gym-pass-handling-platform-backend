using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    public async Task SeedOwnedPassesAsync()
    {
        var utcNow = DateTimeOffset.Now;

        List<GymMembershipPass> passes = [
                new GymMembershipPass {
                    GymMembershipId = _userGymMembership1Id,
                    Type = PassType.SingleUse,
                    TotalUses = 1,
                    RemainingUses = 1,
                    ExpirationDate = null,
                },
                new GymMembershipPass {
                    GymMembershipId = _userGymMembershipId2,
                    Type = PassType.SingleUse,
                    TotalUses = 1,
                    RemainingUses = 0,
                    ExpirationDate = null,
                }
            ];

        await _context.AddRangeAsync(passes);
    }
}
