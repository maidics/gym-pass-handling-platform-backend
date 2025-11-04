using FitPass.Application.Common.Models;

namespace FitPass.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> DoesUserExist(string userId, CancellationToken cancellationToken = default);
    Task<bool> IsInRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task<List<string>?> GetRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> AuthorizeAsync(string userId, string policyName, CancellationToken cancellationToken = default);
    Task<Result> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<(Result result, string? userId)> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<(Result result, string? userId)> CreateUserAsync(string email, CancellationToken cancellationToken = default);
    Task<Result> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result> AddToRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task<Result> RemoveFromRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task<string?> GeneratePasswordResetTokenAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(string userId, string resetToken, string newPassword, CancellationToken cancellationToken = default);
    Task<string?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsEmailInUseAsync(string email, CancellationToken cancellationToken = default);
    Task<Result> AddPasswordToUserWithNoPasswordAsync(string email, string password);
    Task<string?> GenerateEmailConfirmationTokenAsync(string userId);
    Task<Result> ConfirmEmailAsync(string email, string emailConfirmationToken);
    Task<string?> GetEmailByIdAsync(string userId);
    Task<bool> IsUserEmailConfirmed(string userId);
}
