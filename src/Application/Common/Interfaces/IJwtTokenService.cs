using FitPass.Application.Users.DTOs;

namespace FitPass.Application.Common.Interfaces;

public interface IJwtTokenService
{
    public Task<JwtToken> GenerateTokenAsync(string userId, CancellationToken cancellationToken);
}
