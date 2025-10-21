using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FitPass.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<ApplicationUser?> FindUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string role)
    {
        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<List<string>?> GetRolesAsync(ApplicationUser user)
    {
        return [.. await _userManager.GetRolesAsync(user)];
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _userManager.CreateAsync(user, password);

        return result;
    }

    public async Task<(Result result, ApplicationUser? user)> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email);

        cancellationToken.ThrowIfCancellationRequested();

        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
        {
            return (Result.Failure(["Invalid email or password"]), null);
        }

        return (Result.Success(), user);
    }

    public async Task<Result> AddToRoleAsync(ApplicationUser user, string role)
    {
        var result = await _userManager.AddToRoleAsync(user, role);

        return result.ToApplicationResult();
    }

    public async Task<Result> RemoveFromRoleAsync(ApplicationUser user, string role)
    {
        var result = await _userManager.RemoveFromRoleAsync(user, role);

        return result.ToApplicationResult();
    }

    public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        return _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(ApplicationUser user, string resetToken, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        return result.ToApplicationResult();
    }

    public async Task<Result> UpdateSecurityStampAsync(ApplicationUser user)
    {
        var result = await _userManager.UpdateSecurityStampAsync(user);

        return result.ToApplicationResult();
    }
}
