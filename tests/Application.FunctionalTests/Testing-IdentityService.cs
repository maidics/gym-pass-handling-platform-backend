using FitPass.Application.Common.Interfaces;
using FitPass.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public static async Task<List<string>> GetUserRolesAsync(string userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var roles = await identityService.GetRolesAsync(userId);

        Guard.Against.Null(roles);

        return roles;
    }

    public static async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var token = await identityService.GenerateEmailConfirmationTokenAsync(userId);

        Guard.Against.Null(token);

        return token;
    }

    public static async Task<string> GeneratePasswordResetTokenAsync(string userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var token = await identityService.GeneratePasswordResetTokenAsync(userId);

        Guard.Against.Null(token);

        return token;
    }

    public static async Task<string> GetUserIdByEmailAsync(string email)
    {
        using var scope = _scopeFactory.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var userId = await identityService.GetUserIdByEmailAsync(email);

        Guard.Against.Null(userId);

        return userId;
    }
}
