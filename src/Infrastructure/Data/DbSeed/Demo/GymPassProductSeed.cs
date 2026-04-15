using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private async Task SeedGymPassProductsAsync()
    {
        var products = new List<GymPassProduct>
        {
            GymPassProduct
                .SingleUse("DemoGymId", "Single Use Pass Name", "Single Use Pass Description", true, new Money(5, CurrencyCode.EUR)),
            
            GymPassProduct
                .SingleUse("DemoGymId", "Inactive Pass Name", "Inactive Pass Description", false, new Money(5, CurrencyCode.EUR)),
            
            GymPassProduct
                .MultiUse("DemoGymId", "Multi Use Pass Name", "Multi Use Pass Description", 10, true, new Money(5, CurrencyCode.EUR)),
            
            GymPassProduct
                .UnlimitedUse("DemoGymId", "Unlimited Use Pass Name", "Unlimited Use Pass Description", 30, true, new Money(5, CurrencyCode.EUR)),
        };

        await _context.GymPassProducts.AddRangeAsync(products);
    }
}
