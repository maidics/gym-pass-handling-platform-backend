using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Constants;
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

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(ApplicationUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<(Result Result, string UserId)> CreateAppAdminUserAsync(string email, string password, string firstName, string? lastName)
    {
        var user = new ApplicationUser
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            UserGymMemberships = null,
            GymStaffAssigment = null
        };

        var creationResult = await _userManager.CreateAsync(user, password);

        if (!creationResult.Succeeded)
        {
            return (creationResult.ToApplicationResult(), user.Id);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.AppAdministrator);

        return (roleResult.ToApplicationResult(), user.Id);
    }

    public async Task<(Result Result, string UserId)> CreateGymManagementUserAsync(string email, string password, string firstName, string lastName, string role, Gym gym, string escalationEmail)
    {
        var userId = Guid.NewGuid().ToString();

        var user = new ApplicationUser
        {
            Id = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            UserGymMemberships = null,
            GymStaffAssigment = new GymStaffAssigment
            {
                ApplicationUserId = userId,
                GymId = gym.Id,
                Gym = gym,
                EscalationEmail = escalationEmail
            }
        };

        var creationResult = await _userManager.CreateAsync(user, password);

        if (!creationResult.Succeeded)
        {
            return (creationResult.ToApplicationResult(), userId);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);

        return (roleResult.ToApplicationResult(), userId);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<List<string>?> GetRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return null;
        }

        return [..await _userManager.GetRolesAsync(user)];
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

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }
}
