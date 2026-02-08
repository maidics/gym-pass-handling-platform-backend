using System.Text.Json;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.GymPassProducts.Commands;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
using FitPass.Infrastructure.Identity;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public static async Task<TenantPaymentProfile> CreateTenantPaymentProfileAsync(string gymId)
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPaymentTenantService>();

        var result = await service.CreateTenantAccount(
            gymId,
            $"test_connected_tenant_account_{Guid.NewGuid()}@localhost",
            "Test Business",
            CancellationToken.None
        );

        if (!result.Succeeded)
        {
            throw new Exception(
                $"Failed to create tenant payment profile: {string.Join(", ", result.Errors)}"
            );
        }

        var paymentProfile = new TenantPaymentProfile
        {
            GymId = gymId,
            PaymentAccountId = result.Value,
        };

        await AddAsync(paymentProfile);

        return paymentProfile;
    }
}
