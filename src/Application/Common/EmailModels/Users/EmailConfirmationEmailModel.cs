namespace FitPass.Application.Common.EmailModels.Users;

public class EmailConfirmationEmailModel : IEmailModel
{
    public required string Language { get; init; }
    public required string Subject { get; init; }
    public required string Greeting { get; init; }
    public required string Body { get; init; }
    public required string Farewell { get; init; }
}
