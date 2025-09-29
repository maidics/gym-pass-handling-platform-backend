using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private readonly string gymId1 = "localhostGymId1";
    private readonly string gymId2 = "localhostGymId2";
    private readonly string gymId3 = "localhostGymId3";

    private async Task SeedGymAsync()
    {
        var existingGyms = await _context.Gyms.ToListAsync();

        if (existingGyms.Count == 0 || existingGyms.FirstOrDefault(g => g.Id == gymId1 || g.Id == gymId2) == null)
        {
            List<Gym> gyms = [
                    new Gym {
                        Id = gymId1,
                        Name = "LocalhostGymId1",
                        Address = "localhost1",
                        Status = GymStatus.Active,
                        Tier = GymTier.Elite
                    },
                    new Gym {
                        Id = gymId2,
                        Name = "LocalhostGymId2",
                        Address = "localhost2",
                        Status = GymStatus.Active,
                        Tier = GymTier.Local
                    },
                    new Gym {
                        Id = gymId3,
                        Name = "LocalhostGymId3",
                        Address = "localhost3",
                        Status = GymStatus.Inactive,
                        Tier = GymTier.Local
                    }
                ];

            await _context.Gyms.AddRangeAsync(gyms);
        }
    }
}
