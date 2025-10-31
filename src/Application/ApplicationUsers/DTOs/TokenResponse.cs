namespace FitPass.Application.ApplicationUsers.DTOs;

public record JwtToken
{
    public required string AccessToken { get; init; }
    public required int ExpiresIn { get; init; }
    public string TokenType { get; } = "Bearer";
    public string? RefreshToken { get; init; }
}
