namespace FitPass.Application.Common.Interfaces;

public interface ILocalDevEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendPasswordResetEmailAsync(string to, string resetToken, string userId);
}