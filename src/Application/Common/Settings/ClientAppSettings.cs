namespace FitPass.Application.Common.Settings;

public class ClientAppSettings
{
    public required string BaseUrl { get; init; }
    public required string ActivateUserAccountPath { get; init; }

    public string GetEmailConfirmationUrl(string token, string email, bool setPassword)
    {
        return $"{BaseUrl}{ActivateUserAccountPath}?token={Uri.EscapeDataString(token)}&user={Uri.EscapeDataString(email)}&=flag{(setPassword ? 1 : 0)}";
    }
}
