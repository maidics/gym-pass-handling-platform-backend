using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using FitPass.Application.Common.EmailModels;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Strings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using RazorLight;

namespace FitPass.Infrastructure.Email;
public class LocalEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IRazorLightEngine _razorEngine;
    private readonly SmtpClient _smtpClient;
    private readonly ILocalizer _localizer;

    public LocalEmailService(
        IWebHostEnvironment environment, 
        IOptions<EmailSettings> emailOptions, 
        IRazorLightEngine razorEngine,
        ILocalizer localizer)
    {
        _settings = emailOptions.Value;

        if (_settings.EmailPickupFolderName is null || _settings.EmailPickupSubFolderName is null)
        {
            throw new InvalidOperationException("No email pickup folder configured for local email service.");
        }

        var pickupDirectory = Path.Combine(environment.ContentRootPath, "..", "..", _settings.EmailPickupFolderName, _settings.EmailPickupSubFolderName);
        
        if (!Directory.Exists(pickupDirectory))
        {
            Directory.CreateDirectory(pickupDirectory);
        }

        _razorEngine = razorEngine;

        _smtpClient = new SmtpClient
        {
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory, PickupDirectoryLocation = pickupDirectory
        };

        _localizer = localizer;
    }

    public async Task SendEmailAsync(IEmailModel emailModel, string[] to, string[]? cc = null, string[]? bcc = null)
    {
        //var defaultCulture = _localizer.DefaultCulture;
        //using var scope = new CultureInfoScope(defaultCulture); 
        //overriding here because multiple people receiving the email, TODO: ensure the languages are the one that each user prefers?
        //using ensures the scope is disposed even if this method throws
        //this culture is tied to the thread of IRazorLightEngine so this have to be forced
        
        var mailMessage = await GetMailMessage(emailModel);

        foreach (string address in to)
        {
            mailMessage.To.Add(address);
        }

        await _smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendEmailAsync(IEmailModel emailModel, string to)
    {
        var language = emailModel.Language ?? _localizer.DefaultCulture;
        
        var mailMessage = await GetMailMessage(emailModel);

        mailMessage.To.Add(to);
        
        await _smtpClient.SendMailAsync(mailMessage);
    }

    private async Task<MailMessage> GetMailMessage(IEmailModel model)
    {
        var email = (Email)await RenderAsync((dynamic)model);

        return new MailMessage
        {
            From = _settings.NoReplyMailAddress, Subject = email.Subject, Body = email.Html, IsBodyHtml = true,
        };
    } 

    private async Task<Email> RenderAsync<T>(T model) where T : IEmailModel
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
            subject = WebUtility.HtmlDecode(subjectMatch.Groups[1].Value); //ensure correct subject encoding so hungarian characters are displayed properly
        }
        else
        {
            subject = CommonStrings.AppName;
        }

        return new Email(html, subject);
    }

    private record Email(string Html, string Subject);
}
