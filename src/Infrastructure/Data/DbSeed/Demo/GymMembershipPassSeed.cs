using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private async Task SeedGymMembershipPassesAsync()
    {
        var products = await _context.GymPassProducts.ToListAsync();

        var now = DateTimeOffset.UtcNow;

        var passes = products.Select(x => x.ToGymMembershipPass("GymMembershipId", "UserId", now));

        await _context.GymMembershipPasses.AddRangeAsync(passes);
        await _context.SaveChangesAsync();
    }
}
