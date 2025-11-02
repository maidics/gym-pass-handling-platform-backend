using FitPass.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public static async Task<List<string>> GetRoles(string userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var roles = await identityService.GetRolesAsync(userId);

        if (roles is null)
        {
            throw new InvalidOperationException("Failed to retrieve user roles: user not found.");
        }

        return roles;
    }

    public static async Task<string> GenerateEmailConfirmationToken(string userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var token = await identityService.GenerateEmailConfirmationTokenAsync(userId);

        if (token is null)
        {
            throw new InvalidOperationException("Failed to generate token: user not found.");
        }

        return token;
    }
}
