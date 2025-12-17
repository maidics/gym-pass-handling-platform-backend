using FitPass.Application.Common.EmailModels.Users;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Settings;
using FitPass.Domain.Events.Users;

namespace FitPass.Application.Users.EventHandlers;

public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly IIdentityService  _identityService;
    private readonly ClientAppSettings _clientAppSettings;
    private readonly ILocalizer _localizer;
    private readonly IEmailService  _emailService;

    public UserRegisteredEventHandler(
        IIdentityService identityService,
        ClientAppSettings clientAppSettings,
        ILocalizer localizer,
        IEmailService emailService)
    {
        _identityService = identityService;
        _clientAppSettings = clientAppSettings;
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
            AccountActivationUrl = url,
            UserFirstName = notification.UserFirstName
        };

        await _emailService.SendEmailAsync(model, notification.UserEmail);
    }
}
