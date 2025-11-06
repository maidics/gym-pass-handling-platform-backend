using System.Net.Mail;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Strings;
using FitPass.Infrastructure.Services.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitPass.Infrastructure.Services.Email;
public class LocalDevEmailService : IEmailService
{
    private readonly string _pickupDirectory;
    private readonly EmailSettings _settings;
    private readonly ILogger<LocalDevEmailService> _logger;

    public LocalDevEmailService(IWebHostEnvironment environment, IOptions<EmailSettings> emailOptions, ILogger<LocalDevEmailService> logger)
    {
        _logger = logger;

        _pickupDirectory = Path.Combine(environment.ContentRootPath, "EmailPickup");

        _logger.LogInformation($"Local email path set to: {_pickupDirectory}");

        if (!Directory.Exists(_pickupDirectory))
        {
            Directory.CreateDirectory(_pickupDirectory);
        }

        _settings = emailOptions.Value;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        using var smtpClient = new SmtpClient
        {
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
            PickupDirectoryLocation = _pickupDirectory
        };

        var mailMessage = new MailMessage
        {
            From = _settings.NoReplyMailAddress,
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(to);

        return smtpClient.SendMailAsync(mailMessage);
    }

    public Task SendPasswordResetEmailAsync(string to, string resetToken, string userId)
    {
        using var smtpClient = new SmtpClient
        {
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
            PickupDirectoryLocation = _pickupDirectory
        };

        var encodedToken = Uri.EscapeDataString(resetToken);
        var encodedUserId = Uri.EscapeDataString(userId);

        var resetUrl = $"{_settings.PasswordResetUrl}?token={encodedToken}&userId={encodedUserId}";

        var mailMessage = new MailMessage
        {
            From = _settings.NoReplyMailAddress,
            Subject = EmailSubjects.Placeholder(),
            Body = $"Click this to reset your password: {resetUrl}"
        };

        mailMessage.To.Add(to);

        return smtpClient.SendMailAsync(mailMessage);
    }
}