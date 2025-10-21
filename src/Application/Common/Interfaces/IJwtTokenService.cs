using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IJwtTokenService
{
    public Task<TokenResponse> GenerateTokenAsync(ApplicationUser user, CancellationToken cancellationToken);
}
