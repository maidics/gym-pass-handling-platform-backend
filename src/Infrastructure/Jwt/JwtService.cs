using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitPass.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FitPass.Infrastructure.Jwt;

public class JwtService : IJwtService
{
    private readonly IIdentityService _identityService;
    private readonly JwtSettings _settings;
    private readonly TimeProvider _timeProvider;

    public JwtService(
        IIdentityService identityService, 
        IOptions<JwtSettings> options,
        TimeProvider timeProvider)
    {
        _identityService = identityService;
        _settings = options.Value;
        _timeProvider = timeProvider;
    }
    public async Task<Application.Users.DTOs.Jwt> GenerateTokenAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId), //sub is the standard claim for user ID
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) //jti is a unique token identifier
        };

        var userRoles = await _identityService.GetRolesAsync(userId);

        if (userRoles != null)
        {
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var utcNow = _timeProvider.GetUtcNow();

        var expiryMinutes = Convert.ToInt32(_settings.ExpiryInMinutes);
        var expires = utcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            notBefore: utcNow.UtcDateTime,
            expires: expires.UtcDateTime,
            claims: claims,
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(token);

        cancellationToken.ThrowIfCancellationRequested();

        return new Application.Users.DTOs.Jwt
        {
            AccessToken = accessToken,
            ExpiresIn = expiryMinutes * 60
        };
    }
}
