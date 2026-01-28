using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private async Task SeedGymPassUsagesAsync()
    {
        var utcNow = DateTimeOffset.UtcNow;

        var passes = await _context.GymMembershipPasses.ToListAsync();

        if (passes.Count == 0)
        {
            throw new InvalidOperationException($"No passes found for {nameof(GymPassUsage)} seeding.");
        }
        
        var usages = new List<GymPassUsage>()
        {
            //Expired
            new GymPassUsage()
            {
                UserId = "UserId",
                CreatedBy = "GymAdminLocalhostId",
                CreatedOn = utcNow.AddHours(-1.1),
                GymId = "TestGymId",
                GymSessionEndedAt = null,
                PassType = PassType.SingleUse,
                TotalPassUses = 0,
                RemainingPassUses = 0,
                PassExpirationDate = null,
                LockerNumber = "20",
                PassId = passes.FirstOrDefault(x => x.Type == PassType.SingleUse)!.Id,
                PassUseResult = PassUseResult.Expired
            },
            //Success - ongoing
            new GymPassUsage()
            {
                UserId = "UserId",
                CreatedBy = "GymAdminLocalhostId",
                CreatedOn = utcNow.AddHours(-1),
                GymId = "TestGymId",
                GymSessionEndedAt = null,
                PassType = PassType.SingleUse,
                TotalPassUses = 1,
                RemainingPassUses = 0,
                PassExpirationDate = null,
                LockerNumber = "20",
                PassId = passes.FirstOrDefault(x => x.Type == PassType.SingleUse)!.Id,
                PassUseResult = PassUseResult.Success
            },
            //Success - finished 2 days ago
            new GymPassUsage()
            {
                UserId = "UserId",
                CreatedBy = "GymStaffLocalhostId",
                CreatedOn = utcNow.AddDays(-2),
                GymId = "TestGymId",
                GymSessionEndedAt = utcNow.AddDays(-2).AddHours(1),
                PassType = PassType.SingleUse,
                TotalPassUses = 1,
                RemainingPassUses = 0,
                PassExpirationDate = null,
                LockerNumber = "12",
                PassId = passes.FirstOrDefault(x => x.Type == PassType.SingleUse)!.Id,
                PassUseResult = PassUseResult.Success
            },
            //Success - finished two hours ago
            new GymPassUsage()
            {
                UserId = "UserId",
                CreatedBy = "GymStaffLocalhostId",
                CreatedOn = utcNow.AddHours(-3),
                GymId = "TestGymId",
                GymSessionEndedAt = utcNow.AddHours(-2),
                PassType = PassType.SingleUse,
                TotalPassUses = 1,
                RemainingPassUses = 0,
                PassExpirationDate = null,
                LockerNumber = "44",
                PassId = passes.FirstOrDefault(x => x.Type == PassType.SingleUse)!.Id,
                PassUseResult = PassUseResult.Success
            },
        };

        await _context.GymPassUsages.AddRangeAsync(usages);
        await _context.SaveChangesAsync();
    }
}
