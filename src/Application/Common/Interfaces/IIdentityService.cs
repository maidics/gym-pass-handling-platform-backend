using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);
    Task<List<string>?> GetRolesAsync(string userId);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(ApplicationUser user, string password);
    Task<(Result Result, string UserId)> CreateAppAdminUserAsync(string email, string password, string firstName, string? lastName);
    Task<(Result Result, string UserId)> CreateGymManagementUserAsync(string email, string password, string firstName, string lastName, string role, Gym gym, string escalationEmail);
    Task<Result> DeleteUserAsync(string userId);
}