using System.Net.Mail;

namespace FitPass.Infrastructure.Email;

public class EmailSettings
{
    public required string NoReplyEmail { get; set; }
    public required string PasswordResetUrl { get; set; }

    public MailAddress NoReplyMailAddress => new MailAddress(NoReplyEmail);
}