using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

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
                    IsActive = true,
                    Price = Money.Eur(10)
                },
                new GymPassProduct {
                    Id = "Product2",
                    Name = "TestProduct2",
                    Description = "test",
                    GymId = gymId1,
                    Type = PassType.MultiUse,
                    TotalUses = 2,
                    DaysAfterExpiring = 365,
                    IsActive = true,
                    Price = Money.Eur(10)
                },
                new GymPassProduct {
                    Id = "Product3",
                    Name = "TestProduct3",
                    Description = "test",
                    GymId = gymId1,
                    Type = PassType.Unlimited,
                    TotalUses = null,
                    DaysAfterExpiring = 30,
                    IsActive = true,
                    Price = Money.Eur(10)
                },
                new GymPassProduct {
                    Id = "Product4",
                    Name = "TestProduct4",
                    Description = "test",
                    GymId = gymId1,
                    Type = PassType.SingleUse,
                    TotalUses = 1,
                    DaysAfterExpiring = 365,
                    IsActive = false,
                    Price = Money.Eur(10)
                }
            ];

        await _context.GymPassProducts.AddRangeAsync(gymPassProducts);
    }
}
