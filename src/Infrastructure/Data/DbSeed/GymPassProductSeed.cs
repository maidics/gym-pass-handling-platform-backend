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
                    HUFPrice = 2,
                    IsActive = true
                },
                new GymPassProduct {
                    Id = "Product2",
                    GymId = gymId1,
                    Type = PassType.MultiUse,
                    TotalUses = 2,
                    DaysAfterExpiring = 365,
                    HUFPrice = 3,
                    IsActive = true
                },
                new GymPassProduct {
                    Id = "Product3",
                    GymId = gymId1,
                    Type = PassType.Unlimited,
                    TotalUses = null,
                    DaysAfterExpiring = 30,
                    HUFPrice = 4,
                    IsActive = true
                },
                new GymPassProduct {
                    Id = "Product4",
                    GymId = gymId1,
                    Type = PassType.SingleUse,
                    TotalUses = 1,
                    DaysAfterExpiring = 365,
                    HUFPrice = 2,
                    IsActive = false
                }
            ];

        await _context.GymPassProducts.AddRangeAsync(gymPassProducts);
    }
}
