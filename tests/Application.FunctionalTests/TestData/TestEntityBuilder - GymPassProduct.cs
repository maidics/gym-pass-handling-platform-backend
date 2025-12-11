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
        bool isActive = true)
    {
        await RunAsUserAsync(gymAdmin);
        
        var command = new CreateGymPassProductCommand(name, description, type, totalUses, daysAfterExpiring, isActive, price);
        
        string gymPassProductId;

        try
        {
            var result = await SendAsync(command);
            
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create GymPassProduct: {result.Message}");
            }

            gymPassProductId = result.Value.Id;
        } catch (Exception ex)
        {
            throw new Exception("Error creating GymPassProduct", ex);
        }

        var product = await FindAsync<GymPassProduct>(gymPassProductId);

        Guard.Against.Null(product);

        LogOutCurrentUser();

        return product;
    }
}
