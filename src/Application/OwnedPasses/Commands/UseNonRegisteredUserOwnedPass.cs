using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain;
using FitPass.Domain.Constants;

namespace FitPass.Application.OwnedPasses.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record UseNonRegisteredUserOwnedPassCommand
    (
        string NonRegisteredUserId,
        string OwnedPassId
    ) : IRequest;

public class UseNonRegisteredUserOwnedPassCommandValidator : AbstractValidator<UseNonRegisteredUserOwnedPassCommand>
{
    public UseNonRegisteredUserOwnedPassCommandValidator()
    {
        RuleFor(v => v.NonRegisteredUserId).NotEmptyWithMessage(nameof(UseNonRegisteredUserOwnedPassCommand.NonRegisteredUserId));

        RuleFor(v => v.OwnedPassId).NotEmptyWithMessage(nameof(UseNonRegisteredUserOwnedPassCommand.OwnedPassId));
    }
}

public class UseNonRegisteredUserOwnedPassCommandHandler : IRequestHandler<UseNonRegisteredUserOwnedPassCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UseNonRegisteredUserOwnedPassCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    public async Task Handle(UseNonRegisteredUserOwnedPassCommand command, CancellationToken cancellationToken)
    {
        var gymStaffAssigment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        Guard.Against.Null(gymStaffAssigment, "Gym staff assignment", "Failed to find currently logged in Gym Admin or Gym Staff member.");

        var nonRegisteredUser = await _context
            .NonRegisteredUsers
            .Include(nru => nru.UserGymMemberships.Where(ugm => ugm.GymId == gymStaffAssigment.GymId))
            .ThenInclude(ugm => ugm.OwnedPasses.Where(op => op.Id == command.OwnedPassId))
            .FirstOrDefaultAsync(nru => nru.Id == command.NonRegisteredUserId);

        Guard.Against.NotFound(command.NonRegisteredUserId, nonRegisteredUser, "Non registered user id");

        if (nonRegisteredUser.UserGymMemberships.Count == 0)
        {
            throw new BadRequestException("User is not a member of your gym.");
        }

        var pass = nonRegisteredUser.UserGymMemberships.First().OwnedPasses.First();

        Guard.Against.NotFound(command.OwnedPassId, pass, "Owned pass");

        var passUseResult = pass.Use();

        if (passUseResult != PassUseResult.Success)
        {
            throw new BadRequestException("Specified pass is expired or has no uses left.");
        }

        await _context.SaveChangesAsync();
    }
}
