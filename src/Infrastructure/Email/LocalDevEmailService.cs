using System.Net.Mail;
using FitPass.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Fitpass.Infrastructure.Email;
public class LocalDevEmailService : ILocalDevEmailService
{
    private readonly string _pickupDirectory;
    private readonly MailAddress _noreplyAddress = new MailAddress("no-reply@fitpass.com");
    private readonly ILogger<LocalDevEmailService> _logger;

    public LocalDevEmailService(IWebHostEnvironment environment, ILogger<LocalDevEmailService> logger)
    {
        _logger = logger;

        _pickupDirectory = Path.Combine(environment.ContentRootPath, "EmailPickup");

        _logger.LogInformation($"Local email path set to: {_pickupDirectory}");

        if (!Directory.Exists(_pickupDirectory))
        {
            Directory.CreateDirectory(_pickupDirectory);
        }
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
            From = _noreplyAddress,
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(to);

        return smtpClient.SendMailAsync(mailMessage);
    }
}
