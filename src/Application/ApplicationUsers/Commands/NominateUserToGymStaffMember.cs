using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.ApplicationUsers.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record NominateUserToGymStaffMemberCommand(string UserEmailToNominate, string EscalationEmail) : IRequest;

public class NominateUserToGymStaffMemberCommandValidator : AbstractValidator<NominateUserToGymStaffMemberCommand>
{
    public NominateUserToGymStaffMemberCommandValidator()
    {
        RuleFor(v => v.UserEmailToNominate).NotEmptyWithMessage("User's email to nominate");

        RuleFor(v => v.EscalationEmail)
            .NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Email, "Escalation email")
            .NotEqual(v => v.UserEmailToNominate);
    }
}

public class NominateUserToGymStaffMemberCommandHandler : IRequestHandler<NominateUserToGymStaffMemberCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserProfileService _userProfileService;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public NominateUserToGymStaffMemberCommandHandler(IApplicationDbContext context, IUserProfileService userProfileService, IUser user, IIdentityService identityService)
    {
        _context = context;
        _userProfileService = userProfileService;
        _user = user;
        _identityService = identityService;
    }
    public async Task Handle(NominateUserToGymStaffMemberCommand command, CancellationToken cancellationToken)
    {
        var nominatorAssignment = await _userProfileService.GetUserGymStaffAssigmentAsync(_user.Id!, cancellationToken);

        Guard.Against.Null(nominatorAssignment, "Id", "Failed to find currently logged in Gym Admin.");

        var user = await _context
            .ApplicationUsers
            .Include(au => au.GymStaffAssigment)
            .FirstOrDefaultAsync(au => au.Email == command.UserEmailToNominate, cancellationToken);

        Guard.Against.NotFound(command.UserEmailToNominate, user, "Email");

        if (user.GymStaffAssigment != null)
        {
            throw new BadRequestException("Cannot nominate a user that is already a Gym Staff member.");
        }

        if (user.IsGymMember)
        {
            throw new BadRequestException("Cannot nominate a user that is member of a gym. Please ask them to register a new account for this.");
        }

        var newGymStaffAssignment = new GymStaffAssigment
        {
            ApplicationUserId = user.Id,
            GymId = nominatorAssignment.GymId,
            EscalationEmail = command.EscalationEmail,
            Role = Roles.GymStaff
        };

        user.GymStaffAssigment = newGymStaffAssignment;

        var result = await _identityService.AddToRoleAsync(user, Roles.GymStaff);

        if (!result.Succeeded)
        {
            throw new Exception($"Failed to nominate user: {string.Join(", ", result.Errors)}");
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
