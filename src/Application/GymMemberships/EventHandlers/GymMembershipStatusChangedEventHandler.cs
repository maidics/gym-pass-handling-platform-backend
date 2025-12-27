using FitPass.Application.Common.EmailModels.GymMemberships;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Scopes;
using FitPass.Domain.Enums;
using FitPass.Domain.Events.GymMemberships;
using FitPass.Domain.Strings;

namespace FitPass.Application.GymMemberships.EventHandlers;

public class GymMembershipStatusChangedEventHandler : INotificationHandler<GymMembershipStatusChangedEvent>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILocalizer _localizer;
    private readonly IClientNotificationSender  _clientNotificationSender;

    public GymMembershipStatusChangedEventHandler(
        IIdentityService identityService, 
        IApplicationDbContext context,
        IEmailService emailService,
        IClientNotificationSender clientNotificationSender,
        ILocalizer localizer)
    {
        _identityService = identityService;
        _context = context;
        _emailService = emailService;
        _clientNotificationSender = clientNotificationSender;
        _localizer = localizer;
    }
    
    public async Task Handle(GymMembershipStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var userEmail = await _identityService.GetEmailByIdAsync(notification.UserId);

        if (userEmail is null)
        {
            return;
        }
        
        var gymName = await _context.Gyms
            .AsNoTracking()
            .Where(x => x.Id == notification.GymId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync();

        gymName ??= _localizer.Get(nameof(SharedResource.Gym));

        var result = await _context.UserProfiles
            .AsNoTracking()
            .Where(x => x.UserId == notification.UserId)
            .Select(x => new { x.FirstName, x.PreferredLanguage })
            .FirstOrDefaultAsync();
        
        Guard.Against.NullParameterRelatedToCurrentUser(result, nameof(result), notification.UserId);

        var language = result.PreferredLanguage ?? _localizer.DefaultCulture;
        var isBanned = notification.NewStatus == GymMembershipStatus.Banned;

        GymMembershipStatusChangedEmailModel model;

        using (var scope = new CultureInfoScope(language))
        {
            model = new GymMembershipStatusChangedEmailModel
            {
                Language = language,

                Subject = isBanned ? 
                    _localizer.Get(nameof(SharedResource.GymMembershipStatusBannedEmailSubject), gymName) :
                    _localizer.Get(nameof(SharedResource.GymMembershipStatusUnbannedEmailSubject), gymName),

                Greeting = _localizer.Get(nameof(SharedResource.EmailGreeting), result.FirstName ?? _localizer.Get(nameof(SharedResource.User))),

                Body = isBanned ?
                    _localizer.Get(nameof(SharedResource.GymMembershipStatusBannedEmailBody1)) :
                    _localizer.Get(nameof(SharedResource.GymMembershipStatusUnbannedEmailBody1)),

                Body2 = isBanned ?
                    _localizer.Get(nameof(SharedResource.GymMembershipStatusBannedEmailBody2), gymName) :
                    _localizer.Get(nameof(SharedResource.GymMembershipStatusUnbannedEmailBody2), gymName),

                Farewell = _localizer.Get(nameof(SharedResource.EmailFarewell), CommonStrings.AppName)
            };
        }
        
        await _emailService.SendEmailAsync(model, userEmail);

        var message = notification.NewStatus == GymMembershipStatus.Banned
            ? _localizer.GetForCulture(language, nameof(SharedResource.GymMembershipStatusBannedEmailSubject), gymName)
            : _localizer.GetForCulture(language, nameof(SharedResource.GymMembershipStatusUnbannedEmailSubject), gymName);

        var clientNotification = ClientNotification.Create(message, ClientNotificationType.GymMembershipStatusChange);
        
        await _clientNotificationSender.SendAsync(notification.UserId, clientNotification);
    }
}
