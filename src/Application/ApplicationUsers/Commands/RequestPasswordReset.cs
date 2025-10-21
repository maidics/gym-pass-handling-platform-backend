
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Strings;

namespace Fitpass.Application.ApplicationUsers.Commands;

[Authorize]
public record RequestPasswordResetEmailCommand : IRequest;

public class RequestPasswordResetEmailCommandHandler : IRequestHandler<RequestPasswordResetEmailCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ILocalDevEmailService _emailService;

    public RequestPasswordResetEmailCommandHandler(IApplicationDbContext context, IUser user, IIdentityService identityService, ILocalDevEmailService emailService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
        _emailService = emailService;
    }

    public async Task Handle(RequestPasswordResetEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _context
            .ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(au => au.Id == _user.Id!);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        var passwordResetToken = await _identityService.GeneratePasswordResetTokenAsync(user);

        await _emailService.SendEmailAsync(user.Email!, EmailSubjects.Placeholder(), EmailBodies.PasswordReset(passwordResetToken));
    }
}