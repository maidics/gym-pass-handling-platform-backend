using System.Net.Mail;

namespace FitPass.Infrastructure.Email;

public class EmailSettings
{
    public required string? EmailPickupFolderName { get; init; }
    public required string? EmailPickupSubFolderName { get; init; }
    public required string NoReplyAddress { get; init; }
    public required string PasswordResetUrl { get; init; }
    public required string AppEmailModelsNamespace { get; init; }
    public required string InfraRazorModelsNamespace { get; init; }

    public MailAddress NoReplyMailAddress => new MailAddress(NoReplyAddress);
}
