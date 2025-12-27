
namespace FitPass.Application.Users.DTOs;

public record UserDto(
    string Id,
    string FirstName,
    string LastName,
    string? Email,
    string PreferredLanguage,
    DateTimeOffset CreatedOn,
    string[] Roles,
    bool IsEmailConfirmed);
