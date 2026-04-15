using FitPass.Application.Users.DTOs;

namespace FitPass.Application.Common.Interfaces;

public interface IJwtService
{
    public Task<Jwt> GenerateTokenAsync(string userId, CancellationToken cancellationToken);
}
