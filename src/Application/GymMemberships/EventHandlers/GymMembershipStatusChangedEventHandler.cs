using FitPass.Application.Common.EmailModels.GymMemberships;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Enums;
using FitPass.Domain.Events.GymMemberships;
using FitPass.Infrastructure.Localization.Resources;

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

        if (gymName is null)
        {
            return;
        }

        var result = await _context.UserProfiles
            .AsNoTracking()
            .Where(x => x.UserId == notification.UserId)
            .Select(x => new { x.FirstName, x.PreferredLanguage })
            .FirstOrDefaultAsync();
        
        Guard.Against.NullParameterRelatedToCurrentUser(result, nameof(result), notification.UserId);

        var model = new GymMembershipStatusChangedEmailModel
        {
            Language = result.PreferredLanguage,
            NewGymMembershipStatus = notification.NewStatus,
            UserFirstName = result.FirstName,
            GymName = gymName,
        };
        
        await _emailService.SendEmailAsync(model, userEmail);

        var message = model.NewGymMembershipStatus == GymMembershipStatus.Banned
            ? _localizer.GetForCulture(result.PreferredLanguage, nameof(SharedResource.GymMembershipStatusBannedEmailSubject), model.GymName)
            : _localizer.GetForCulture(result.PreferredLanguage, nameof(SharedResource.GymMembershipStatusUnbannedEmailSubject), model.GymName);

        var clientNotification = ClientNotification.Create(message, ClientNotificationType.GymMembershipStatusChange);
        
        await _clientNotificationSender.SendAsync(notification.UserId, clientNotification);
    }
}
