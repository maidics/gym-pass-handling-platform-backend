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
                    Name = "TestProduct1",
                    Description = "test",
                    GymId = gymId1,
                    Type = PassType.SingleUse,
                    TotalUses = 1,
                    DaysAfterExpiring = 365,
                    HufPrice = 2,
                    IsActive = true
                },
                new GymPassProduct {
                    Id = "Product2",
                    Name = "TestProduct2",
                    Description = "test",
                    GymId = gymId1,
                    Type = PassType.MultiUse,
                    TotalUses = 2,
                    DaysAfterExpiring = 365,
                    HufPrice = 3,
                    IsActive = true
                },
                new GymPassProduct {
                    Id = "Product3",
                    Name = "TestProduct3",
                    Description = "test",
                    GymId = gymId1,
                    Type = PassType.Unlimited,
                    TotalUses = null,
                    DaysAfterExpiring = 30,
                    HufPrice = 4,
                    IsActive = true
                },
                new GymPassProduct {
                    Id = "Product4",
                    Name = "TestProduct4",
                    Description = "test",
                    GymId = gymId1,
                    Type = PassType.SingleUse,
                    TotalUses = 1,
                    DaysAfterExpiring = 365,
                    HufPrice = 2,
                    IsActive = false
                }
            ];

        await _context.GymPassProducts.AddRangeAsync(gymPassProducts);
    }
}
