using FitPass.Application.Common.EmailModels.GymMemberships;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Events.GymMemberships;

namespace FitPass.Application.GymMemberships.EventHandlers;

public class GymMembershipStatusChangedEventHandler : INotificationHandler<GymMembershipStatusChangedEvent>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public GymMembershipStatusChangedEventHandler(
        IIdentityService identityService, 
        IApplicationDbContext context,
        IEmailService emailService)
    {
        _identityService = identityService;
        _context = context;
        _emailService = emailService;
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

        var userFirstName = await _context.UserProfiles
            .AsNoTracking()
            .Where(x => x.UserId == notification.UserId)
            .Select(x => x.FirstName)
            .FirstOrDefaultAsync();

        if (userFirstName is null)
        {
            userFirstName = "Felhasználó";
        }

        var model = new GymMembershipStatusChangedEmailModel(notification.NewStatus, userFirstName, gymName);
        
        await _emailService.SendEmailAsync(model, [userEmail]);
    }
}
