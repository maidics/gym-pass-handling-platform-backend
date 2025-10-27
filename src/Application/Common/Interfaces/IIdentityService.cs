using FitPass.Application.Common.Models;
using FitPass.Domain.Constants;

namespace FitPass.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> IsInRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task<List<string>?> GetRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> AuthorizeAsync(string userId, string policyName, CancellationToken cancellationToken = default);
    Task<Result> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> CreateUserAsync(string email, string password, string firstName, string lastName, string role = Roles.User, CancellationToken cancellationToken = default);
    Task<Result> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result> AddToRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task<Result> RemoveFromRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task<string?> GeneratePasswordResetTokenAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(string userId, string resetToken, string newPassword, CancellationToken cancellationToken = default);
    Task<string?> GetUserIdByEmail(string email, CancellationToken cancellationToken = default);
}
