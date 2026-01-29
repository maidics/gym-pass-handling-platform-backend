using FitPass.Application.Common.Models;

namespace FitPass.Application.Common.Interfaces;

public interface IIdentityService
{
    //UserManager (used by IdentityService) does not consume cancellation tokens
    Task<bool> DoesUserExist(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<List<string>?> GetRolesAsync(string userId);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<Result> DeleteUserAsync(string userId);
    Task<(Result result, string? userId)> CreateUserAsync(string email, string password);
    Task<(Result result, string? userId)> CreateUserAsync(string email);
    Task<Result> AuthenticateUserAsync(string email, string password);
    Task<Result> AddToRoleAsync(string userId, string role);
    Task<Result> RemoveFromRoleAsync(string userId, string role);
    Task<string?> GeneratePasswordResetTokenAsync(string userId);
    Task<Result> ResetPasswordAsync(string userId, string resetToken, string newPassword);
    Task<string?> GetUserIdByEmailAsync(string email);
    Task<bool> IsEmailInUseAsync(string email);
    Task<Result> AddPasswordToUserWithNoPasswordAsync(string email, string password);
    Task<bool> DoesUserHavePassword(string userId);
    Task<string?> GenerateEmailConfirmationTokenAsync(string userId);
    Task<Result> ConfirmEmailAsync(string email, string emailConfirmationToken);
    Task<string?> GetEmailByIdAsync(string userId);
    Task<bool> IsUserEmailConfirmed(string userId);
    Task<Result> UpdateUserPasswordAsync(string userId, string currentPassword, string newPassword);
}
