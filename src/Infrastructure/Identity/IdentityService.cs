using System.Transactions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FitPass.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<ApplicationUser?> FindUserByIdAsync(string userId, CancellationToken? cancellationToken = null)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<ApplicationUser?> FindUserByEmailAsync(string email, CancellationToken? cancellationToken = null)
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
        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TimeSpan.FromSeconds(30)
        };

        using var scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var passwordChangeResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);   

            if (!passwordChangeResult.Succeeded)
            {
                _logger.LogError("Failed to change password for user with id: {UserId}, IdentityErrors: {@IdentityErrors}", user.Id, passwordChangeResult.Errors);
                throw new TransactionException($"Failed to change password for user with id: {user.Id}");
            }

            var securityStampUpdateResult = await UpdateSecurityStampAsync(user);

            if (!securityStampUpdateResult.Succeeded)
            {
                _logger.LogError("Failed to update security stamp for user with id: {UserId}, IdentityErrors: {@IdentityErrors}", user.Id, securityStampUpdateResult);
                throw new TransactionException($"Failed to update security stamp for user with id: {user.Id}");
            }

            scope.Complete();

            return securityStampUpdateResult.ToApplicationResult();
        } catch (Exception ex)
        {
            _logger.LogError("Caught exception during user password reset. User id: {UserId}, Exception: {@Exception}", user.Id, ex);
            throw;
        }
    }

    private async Task<IdentityResult> UpdateSecurityStampAsync(ApplicationUser user)
    {
        IdentityResult result = IdentityResult.Failed();

        int retryCount = 3;
        int secondsBeforeRetrying = 5;

        for (var i = 0; i < retryCount; i++)
        {
            result = await _userManager.UpdateSecurityStampAsync(user);

            if (result.Succeeded)
            {
                break;
            }

            var hasRetryableError = result.Errors.Any(e =>
                e.Code.Contains("Concurrency") ||
                e.Code.Contains("Timeout"));

            if (!hasRetryableError)
            {
                _logger.LogError("Updating Security Stamps failed for '{UserId}', IdentityResult: {@Result}, attempt: {Attempt}/{MaxAttempts}", user.Id, result, i + 1, retryCount);
                break;
            }

            _logger.LogError("Updating Security Stamps failed for '{UserId}', IdentityResult: {@Result}, attempt: {Attempt}/{MaxAttempts}", user.Id, result, i + 1, retryCount);
            await Task.Delay(TimeSpan.FromSeconds(secondsBeforeRetrying) * (i + 1));
        }

        return result;
    }
}
