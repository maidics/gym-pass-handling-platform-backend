namespace FitPass.Application.Common.EmailModels.Users;

public class WelcomeEmailModel : IEmailModel
{
    public required string? Language { get; set; }
    public required string AccountActivationUrl { get; init; }
    public required string UserFirstName { get; init; }
}
