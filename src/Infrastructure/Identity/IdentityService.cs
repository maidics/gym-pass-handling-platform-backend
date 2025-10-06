using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FitPass.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IConfiguration _configuration;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _configuration = configuration;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string role)
    {
        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<List<string>?> GetRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return null;
        }

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

    public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _userManager.CreateAsync(user, password);

        return result;
    }

    public async Task<string> GenerateJWTTokenAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user.Id == null)
        {
            throw new Exception("User id is null");
        }

        if (user.Email == null)
        {
            throw new Exception("User email is null");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id), //sub is the standard claim for user ID
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) //jti is a unique token identifier
        };

        var userRoles = await _userManager.GetRolesAsync(user);

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:ExpiryInMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(token);

        cancellationToken.ThrowIfCancellationRequested();

        return accessToken;
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
}
