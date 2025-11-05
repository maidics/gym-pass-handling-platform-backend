using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymMembershipPasses.Commands;

[Authorize(Roles = Roles.User)]
public record UseGymMembershipPassCommand(string GymMembershipPassId) : IRequest<PassUseResult>;

public class UseGymMembershipPassCommandValidator : AbstractValidator<UseGymMembershipPassCommand>
{
    public UseGymMembershipPassCommandValidator()
    {
        RuleFor(v => v.GymMembershipPassId)
            .NotEmptyWithMessage(nameof(UseGymMembershipPassCommand.GymMembershipPassId));
    }
}

public class UseGymMembershipPassCommandHandler : IRequestHandler<UseGymMembershipPassCommand, PassUseResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<UseGymMembershipPassCommand> _logger;

    public UseGymMembershipPassCommandHandler(
        IApplicationDbContext context, 
        IUser user, 
        ILogger<UseGymMembershipPassCommand> logger)
    {
        _context = context;
        _user = user;
        _logger = logger;
    }

    public async Task<PassUseResult> Handle(UseGymMembershipPassCommand command, CancellationToken cancellationToken)
    {
        var pass = await _context
            .GymMembershipPasses
            .Include(op => op.GymMembership)
            .FirstOrDefaultAsync(pass => pass.Id == command.GymMembershipPassId, cancellationToken);

        Guard.Against.NotFound(command.GymMembershipPassId, pass, "Id");
        
        if (_user.Id != pass.GymMembership.ApplicationUserId)
        {
            throw new ForbiddenAccessException();
        }

        var passUseResult = pass.Use();

        if (passUseResult == PassUseResult.AlreadyExpired)
        {
            LogCriticalMessages.UserRequestedToUseAnAlreadyExpiredPass(
                _logger,
                _user.Id,
                pass.Id);
        }

        await _context.SaveChangesAsync();

        return passUseResult;
    }
}
