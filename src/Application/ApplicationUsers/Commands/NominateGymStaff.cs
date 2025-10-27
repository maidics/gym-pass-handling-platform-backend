using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record NominateGymStaffCommand(string UserEmailToNominate, string EscalationEmail) : IRequest;

public class NominateGymStaffCommandValidator : AbstractValidator<NominateGymStaffCommand>
{
    public NominateGymStaffCommandValidator()
    {
        RuleFor(v => v.UserEmailToNominate)
            .NotEmptyWithMaxLenghtAndMessage(nameof(NominateGymStaffCommand.UserEmailToNominate), MaxStringLengths.Email)
            .ValidEmailAddress(nameof(NominateGymStaffCommand.UserEmailToNominate));

        RuleFor(v => v.EscalationEmail)
            .NotEmptyWithMaxLenghtAndMessage(nameof(NominateGymStaffCommand.EscalationEmail), MaxStringLengths.Email)
            .NotEqual(v => v.UserEmailToNominate)
            .WithMessage(ErrorMessages.PropertyMustNotEqualToAnotherProperty(nameof(NominateGymStaffCommand.UserEmailToNominate), nameof(NominateGymStaffCommand.EscalationEmail)))
            .ValidEmailAddress(nameof(NominateGymStaffCommand.EscalationEmail));
    }
}

public class NominateGymStaffCommandHandler : IRequestHandler<NominateGymStaffCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ILogger<NominateGymStaffCommandHandler> _logger;

    public NominateGymStaffCommandHandler(IApplicationDbContext context, IUser user, IIdentityService identityService, ILogger<NominateGymStaffCommandHandler> logger)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
        _logger = logger;
    }
    public async Task Handle(NominateGymStaffCommand command, CancellationToken cancellationToken)
    {
        var nominatorAssignment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id);

        if (nominatorAssignment == null)
        {
            _logger.LogCritical("CRITICAL ERROR: Logged in GymAdmin's GymStaffAssignment ({GymAdminId}) not found.", _user.Id);
            throw new UnauthorizedAccessException();
        }

        var userToNominateId = await _identityService.GetUserIdByEmail(command.UserEmailToNominate);

        Guard.Against.NotFound(command.UserEmailToNominate, userToNominateId, "User");

        var userToNominateRoles = await _identityService.GetRolesAsync(userToNominateId);

        if (userToNominateRoles == null || userToNominateRoles.First() != Roles.PendingGymEmployee)
        {
            throw new BadRequestException("Account with this email is not eligible for GymStaff nomination. Please register a new gym management account for this action");
        }

        var roleResult = await _identityService.ReplaceUserRole(userToNominateId, Roles.PendingGymEmployee, Roles.GymStaff);

        var gymStaffGymStaffAssignment = new GymEmployment
        {
            ApplicationUserId = userToNominateId,
            GymId = nominatorAssignment.GymId,
            Role = Roles.GymStaff
        };
            
        await _context.SaveChangesAsync();
    }
}
