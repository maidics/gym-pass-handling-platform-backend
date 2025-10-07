using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FitPass.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(ApplicationUser user, string role);
    Task<List<string>?> GetRolesAsync(string userId);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<Result> DeleteUserAsync(ApplicationUser user);
    Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken);
    Task<string> GenerateJWTTokenAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task<(Result result, ApplicationUser? user)> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result> AddToRoleAsync(ApplicationUser user, string role);
    Task<Result> RemoveFromRoleAsync(ApplicationUser user, string role);
}
