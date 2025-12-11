using System.Net.Mail;
using System.Text.RegularExpressions;
using FitPass.Application.Common.EmailModels;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Strings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RazorLight;

namespace FitPass.Infrastructure.Email;
public class LocalEmailService : IEmailService
{
    private readonly string _pickupDirectory;
    private readonly EmailSettings _settings;
    private readonly IRazorLightEngine _razorEngine;
    private readonly SmtpClient _smtpClient;

    public LocalEmailService(
        IWebHostEnvironment environment, 
        IOptions<EmailSettings> emailOptions, 
        IRazorLightEngine razorEngine)
    {
        _settings = emailOptions.Value;

        if (_settings.EmailPickupFolderName is null || _settings.EmailPickupSubFolderName is null)
        {
            throw new InvalidOperationException("No email pickup folder configured for local email service.");
        }

        _pickupDirectory = Path.Combine(environment.ContentRootPath, "..", "..", _settings.EmailPickupFolderName, _settings.EmailPickupSubFolderName);
        
        if (!Directory.Exists(_pickupDirectory))
        {
            Directory.CreateDirectory(_pickupDirectory);
        }

        _razorEngine = razorEngine;

        _smtpClient = new SmtpClient
        {
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory, PickupDirectoryLocation = _pickupDirectory
        };
    }

    public async Task SendEmailAsync(IEmailModel emailModel, string[] to, string[]? cc = null, string[]? bcc = null)
    {
        var mailMessage = await GetMailMessage(emailModel);

        foreach (string address in to)
        {
            mailMessage.To.Add(address);
        }

        await _smtpClient.SendMailAsync(mailMessage);
    }

    private async Task<MailMessage> GetMailMessage(IEmailModel model)
    {
        var (html, subject) = await RenderAsync(model);

        return new MailMessage
        {
            From = _settings.NoReplyMailAddress, Subject = subject, Body = html, IsBodyHtml = true,
        };
    } 

    private async Task<(string html, string subject)> RenderAsync<T>(T model) where T : IEmailModel
    {
        var modelNamespace = typeof(T).Namespace;
        
        Guard.Against.Null(input: modelNamespace, message: $"No namespace found for '{typeof(T)}' model.");

        string templateNamespace =
            modelNamespace.Replace(_settings.AppEmailModelsNamespace, _settings.InfraRazorModelsNamespace);

        var templateKey = $"{templateNamespace}.{typeof(T).Name}.cshtml";

        var html = await _razorEngine.CompileRenderAsync(templateKey, model);

        var subjectMatch = Regex.Match(html, @"<title>(.*?)</title>", RegexOptions.IgnoreCase);

        string subject;

        if (subjectMatch.Success)
        {
            subject = subjectMatch.Groups[1].Value;
        }
        else
        {
            subject = CommonStrings.AppName;
        }

        return (html, subject);
    }
}
