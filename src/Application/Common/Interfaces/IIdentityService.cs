using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FitPass.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<ApplicationUser?> FindUserByIdAsync(string userId, CancellationToken? cancellationToken = null);
    Task<ApplicationUser?> FindUserByEmailAsync(string email, CancellationToken? cancellationToken = null);
    Task<bool> IsInRoleAsync(ApplicationUser user, string role);
    Task<List<string>?> GetRolesAsync(ApplicationUser user);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<Result> DeleteUserAsync(ApplicationUser user);
    Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken);
    Task<(Result result, ApplicationUser? user)> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result> AddToRoleAsync(ApplicationUser user, string role);
    Task<Result> RemoveFromRoleAsync(ApplicationUser user, string role);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<Result> ResetPasswordAsync(ApplicationUser user, string resetToken, string newPassword);
}
