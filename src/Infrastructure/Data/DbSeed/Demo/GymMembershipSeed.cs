using FitPass.Domain.Entities;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private async Task SeedGymMembershipsAsync()
    {
        var memberships = new List<GymMembership> { 
            new GymMembership
            {
                Id = "GymMembershipId",
                UserId = "UserId",
                GymId = "TestGymId"
            } 
        };
        
        await _context.GymMemberships.AddRangeAsync(memberships);
        await _context.SaveChangesAsync();
    }
}
