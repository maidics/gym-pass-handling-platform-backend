using FitPass.Application.Common.EmailModels.Users;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Settings;
using FitPass.Domain.Events.Users;
using FitPass.Domain.Strings;
using FitPass.Application.Common.Resources;
using Microsoft.Extensions.Options;

namespace FitPass.Application.Users.EventHandlers;

public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly IIdentityService  _identityService;
    private readonly ClientAppSettings _clientAppSettings;
    private readonly ILocalizer _localizer;
    private readonly IEmailService  _emailService;

    public UserRegisteredEventHandler(
        IIdentityService identityService,
        IOptions<ClientAppSettings> options,
        ILocalizer localizer,
        IEmailService emailService)
    {
        _identityService = identityService;
        _clientAppSettings = options.Value;
        _localizer = localizer;
        _emailService = emailService;
    }
    
    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var token = await _identityService.GenerateEmailConfirmationTokenAsync(notification.UserId);
        
        Guard.Against.Null(token, nameof(token), $"Token generation failed for '{notification.UserEmail}'.");
        
        var url = _clientAppSettings.GetEmailConfirmationUrl(token, notification.UserEmail, notification.ByGymEmployee);

        var model = new WelcomeEmailModel
        {
            Language = _localizer.DefaultCulture,
            Subject = _localizer.Get(nameof(SharedResource.WelcomeEmailSubject), CommonStrings.AppName),
            Greeting = _localizer.Get(nameof(SharedResource.EmailGreeting), notification.UserFirstName),
            Body = _localizer.Get(nameof(SharedResource.WelcomeEmailBody), CommonStrings.AppName, url),
            Farewell = _localizer.Get(nameof(SharedResource.EmailFarewell), CommonStrings.AppName)
        };

        await _emailService.SendEmailAsync(model, notification.UserEmail);
    }
}
