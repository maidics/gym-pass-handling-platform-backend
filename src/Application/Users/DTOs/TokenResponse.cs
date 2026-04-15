namespace FitPass.Application.Users.DTOs;

public record Jwt
{
    public required string AccessToken { get; init; }
    public required int ExpiresIn { get; init; }
    public string TokenType { get; } = "Bearer";
    public string? RefreshToken { get; init; }
}
