using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Events.Users;

namespace FitPass.Application.ApplicationUsers.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record NominateGymStaffCommand(string UserEmailToNominate, string EscalationEmail) : IRequest;

public class NominateGymStaffCommandValidator : AbstractValidator<NominateGymStaffCommand>
{
    public NominateGymStaffCommandValidator()
    {
        RuleFor(v => v.UserEmailToNominate).NotEmptyWithMessage("User's email to nominate");

        RuleFor(v => v.EscalationEmail)
            .NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Email, "Escalation email")
            .NotEqual(v => v.UserEmailToNominate);
    }
}

public class NominateGymStaffCommandHandler : IRequestHandler<NominateGymStaffCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public NominateGymStaffCommandHandler(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }
    public async Task Handle(NominateGymStaffCommand command, CancellationToken cancellationToken)
    {
        var nominatorAssignment = await _context.GymStaffAssigments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id);

        var user = await _context
            .ApplicationUsers
            .Include(au => au.GymStaffAssignment)
            .FirstOrDefaultAsync(au => au.Email == command.UserEmailToNominate);

        Guard.Against.NotFound(command.UserEmailToNominate, user, "Email");

        if (user.GymStaffAssignment == null || user.GymStaffAssignment!.Role != Roles.PendingGymManagement)
        {
            throw new BadRequestException("Account with this email is not eligible for GymStaff nomination. Please register a new gym management account for this action");
        }

        var demotionResult = await _identityService.RemoveFromRoleAsync(user, Roles.PendingGymManagement);

        if (!demotionResult.Succeeded)
        {
            throw new Exception($"Failed to remove user from pending gym management role: {string.Join(", ", demotionResult.Errors)}.");
        }

        var nominationResult = await _identityService.AddToRoleAsync(user, Roles.GymStaff);

        if (!nominationResult.Succeeded)
        {
            throw new Exception($"Failed to nominate user: {string.Join(", ", nominationResult.Errors)}");
        }

        user.GymStaffAssignment.GymId = nominatorAssignment!.GymId;
        user.GymStaffAssignment.Role = Roles.GymStaff;
        user.GymStaffAssignment.EscalationEmail = command.EscalationEmail;

        user.AddDomainEvent(new GymStaffNominatedEvent(user));

        await _context.SaveChangesAsync();
    }
}
