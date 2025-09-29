using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitPass.Domain;
using FitPass.Domain.Enums;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    public async Task SeedOwnedPassesAsync()
    {
        var utcNow = DateTimeOffset.Now;

        List<OwnedPass> ownedPasses = [
                new OwnedPass {
                    UserGymMembershipId = _userGymMembership1Id,
                    Type = PassType.SingleUse,
                    TotalUses = 1,
                    RemainingUses = 1,
                    ExpirationDate = null,
                    EurPrice = 1,
                },
                new OwnedPass {
                    UserGymMembershipId = _userGymMembershipId2,
                    Type = PassType.SingleUse,
                    TotalUses = 1,
                    RemainingUses = 0,
                    ExpirationDate = null,
                    EurPrice = 0,
                }
            ];

        await _context.AddRangeAsync(ownedPasses);
    }
}
