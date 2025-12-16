namespace FitPass.Application.UserProfiles.DTOs;

public record UserProfileWithEmailDto (
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    string PreferredLanguage,
    DateTimeOffset CreatedOn);
