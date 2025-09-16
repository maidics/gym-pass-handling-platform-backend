using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace Fitpass.Application.UserGymMemberships.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record UpdateUserGymMembershipStatusCommand(string ApplicationUserId, string GymId, GymMembershipStatus NewStatus) : IRequest<Result>;

public class UpdateUserGymMembershipStatusCommandValidator : AbstractValidator<UpdateUserGymMembershipStatusCommand>
{
    public UpdateUserGymMembershipStatusCommandValidator()
    {
        RuleFor(v => v.ApplicationUserId).NotEmptyWithMessage("User id");

        RuleFor(v => v.GymId).NotEmptyWithMessage("Gym id");

        RuleFor(v => v.NewStatus).NotEmptyWithMessage("New gym membership status");
    }
}

public class UpdateUserGymMembershipStatusCommandHandler : IRequestHandler<UpdateUserGymMembershipStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserGymMembershipStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Result> Handle(UpdateUserGymMembershipStatusCommand command, CancellationToken cancellationToken)
    {
        var userGymMembership = await _context.UserGymMemberships.FindAsync(command.ApplicationUserId, command.GymId, cancellationToken);

        if (userGymMembership == null)
        {
            return Result.Failure(["User's gym membership not found."]);
        }

        userGymMembership.GymMembershipStatus = command.NewStatus;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}