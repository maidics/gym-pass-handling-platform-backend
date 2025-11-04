using System.Transactions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Strings;
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

    public async Task<bool> IsInRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<List<string>?> GetRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user == null ? null : [.. await _userManager.GetRolesAsync(user)];
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName, CancellationToken cancellationToken = default)
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

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure([ErrorMessages.UserNotFound()]);
        }

        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<(Result result, string? userId)> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), result.Succeeded ? user.Id : null);
    }

    public async Task<(Result result, string? userId)> CreateUserAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
        };

        var result = await _userManager.CreateAsync(user);

        return (result.ToApplicationResult(), result.Succeeded ? user.Id : null);
    }

    public async Task<Result> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return Result.Failure([ErrorMessages.InvalidCredentials()]);
        }

        if (user.PasswordHash == null)
        {
            return Result.Failure([ErrorMessages.UserAccountIsNotActivated()]);
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            return Result.Failure([ErrorMessages.InvalidCredentials()]);
        }

        return Result.Success();
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user == null ? null : await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(string userId, string resetToken, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure([ErrorMessages.UserNotFound()]);
        }

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

    public async Task<string?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email);

        return user is null ? null : user.Id;
    }

    public async Task<Result> AddToRoleAsync(string userId, string role, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure([ErrorMessages.UserNotFound()]);
        }

        var result = await _userManager.AddToRoleAsync(user, role);

        return result.ToApplicationResult(); 
    }

    public async Task<Result> RemoveFromRoleAsync(string userId, string role, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure([ErrorMessages.UserNotFound()]);
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role);

        return result.ToApplicationResult();
    }

    public async Task<bool> IsEmailInUseAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        return user != null;
    }

    public async Task<bool> DoesUserExist(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null;
    }

    public async Task<Result> AddPasswordToUserWithNoPasswordAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return Result.Failure([ErrorMessages.UserNotFound()]);
        }

        if (user.PasswordHash != null)
        {
            return Result.Failure([ResultErrorMessages.UserAlreadyHasPassword()]);
        }

        var result = await _userManager.AddPasswordAsync(user, password);

        return result.ToApplicationResult();
    }

    public async Task<string?> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return null;
        }

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<Result> ConfirmEmailAsync(string email, string emailConfirmationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return Result.Failure([ErrorMessages.UserNotFound()]);
        }

        var result = await _userManager.ConfirmEmailAsync(user, emailConfirmationToken);

        if (!result.Succeeded && result.Errors.Any(e => e.Code == "Invalidtoken"))
        {
            return Result.Failure([ErrorMessages.TokenIsInvalid("Error confirmation")]);
        }

        return result.ToApplicationResult();
    }

    public async Task<string?> GetEmailByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user == null ? null : user.Email;
    }
    
    public async Task<bool> IsUserEmailConfirmed(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && user.EmailConfirmed;
    }
}
