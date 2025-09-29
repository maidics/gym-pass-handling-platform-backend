using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    public async Task SeedGymPassProductsAsync()
    {
        List<GymPassProduct> gymPassProducts = [
                new GymPassProduct {
                    Id = "Product1",
                    GymId = gymId1,
                    Type = PassType.SingleUse,
                    TotalUses = 1,
                    DaysAfterExpiring = 365,
                    EurPrice = 2,
                    IsAvailable = true
                },
                new GymPassProduct {
                    Id = "Product2",
                    GymId = gymId1,
                    Type = PassType.MultiUse,
                    TotalUses = 2,
                    DaysAfterExpiring = 365,
                    EurPrice = 3,
                    IsAvailable = true
                },
                new GymPassProduct {
                    Id = "Product3",
                    GymId = gymId1,
                    Type = PassType.Unlimited,
                    TotalUses = null,
                    DaysAfterExpiring = 30,
                    EurPrice = 4,
                    IsAvailable = true
                },
                new GymPassProduct {
                    Id = "Product4",
                    GymId = gymId1,
                    Type = PassType.SingleUse,
                    TotalUses = 1,
                    DaysAfterExpiring = 365,
                    EurPrice = 2,
                    IsAvailable = false
                }
            ];

        await _context.GymPassProducts.AddRangeAsync(gymPassProducts);
    }
}
