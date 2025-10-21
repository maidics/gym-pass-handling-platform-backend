using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Services.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FitPass.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IIdentityService _identityService;
    private readonly JwtSettings _settings;

    public JwtTokenService(IIdentityService identityService, IOptions<JwtSettings> options)
    {
        _identityService = identityService;
        _settings = options.Value;
    }
    public async Task<TokenResponse> GenerateTokenAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id), //sub is the standard claim for user ID
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) //jti is a unique token identifier
        };

        var userRoles = await _identityService.GetRolesAsync(user);

        if (userRoles != null)
        {
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var utcNow = DateTime.UtcNow;

        var expiryMinutes = Convert.ToInt32(_settings.ExpiryInMinutes);
        var expires = utcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            notBefore: utcNow,
            expires: expires,
            claims: claims,
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(token);

        cancellationToken.ThrowIfCancellationRequested();

        return new TokenResponse
        {
            AccessToken = accessToken,
            ExpiresIn = expiryMinutes * 60
        };
    }
}