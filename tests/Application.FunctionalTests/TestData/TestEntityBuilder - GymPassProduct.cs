using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.GymPassProducts.Commands;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.TestData;

using static Testing;

public partial class TestEntityBuilder
{
    public static async Task<GymPassProduct> BuildGymPassProduct(
        ApplicationUser gymAdmin,
        Money price,
        string name = "Test Gym Pass Product",
        string description = "Test Description",
        PassType type = PassType.SingleUse,
        int? totalUses = 1,
        int? daysAfterExpiring = null,
        bool isActive = true
    )
    {
        await RunAsUserAsync(gymAdmin);

        var command = new CreateGymPassProductCommand(
            name,
            description,
            type,
            totalUses,
            daysAfterExpiring,
            isActive,
            price
        );

        var result = await SendAsync(command);

        var product = await FindAsync<GymPassProduct>(result.Value.Id);

        Guard.Against.Null(product);

        LogOutCurrentUser();

        return product;
    }
}
