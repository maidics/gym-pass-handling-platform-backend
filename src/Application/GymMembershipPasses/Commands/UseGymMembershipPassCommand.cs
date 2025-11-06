using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMembershipPasses.Commands;

[Authorize]
public record UseGymMembershipPassCommand(string GymMembershipPassId) : IRequest;

public class UseGymMembershipPassCommandValidator : AbstractValidator<UseGymMembershipPassCommand>
{
    public UseGymMembershipPassCommandValidator()
    {
        RuleFor(v => v.GymMembershipPassId)
            .NotEmptyWithMessage(nameof(UseGymMembershipPassCommand.GymMembershipPassId));
    }
}

public class UseGymMembershipPassCommandHandler : IRequestHandler<UseGymMembershipPassCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UseGymMembershipPassCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UseGymMembershipPassCommand command, CancellationToken cancellationToken)
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

        if (passUseResult == PassUseResult.Success)
        {
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new BadRequestException("Pass is expired or has no uses left.");
        }
    }
}
