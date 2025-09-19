using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymAdministratorUserCommand
(
    string GymId,
    string GymAdminFirstName,
    string GymAdminLastName,
    string GymAdminEmail,
    string GymAdminPassword,
    string GymAdminPasswordConfirm,
    string EscalationEmail
) : IRequest<Result>;

public class CreateGymAdministratorUserCommandValidator : AbstractValidator<CreateGymAdministratorUserCommand>
{
    public CreateGymAdministratorUserCommandValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage("Gym id");

        RuleFor(v => v.GymAdminFirstName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Gym admin first name");

        RuleFor(v => v.GymAdminLastName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Gym admin last name");

        RuleFor(v => v.GymAdminEmail)
            .NotEmptyWithMessage("Gym admin email")
            .EmailAddress().WithMessage("A valid email address must be provided.");

        RuleFor(v => v.GymAdminPassword)
            .NotEmptyWithMessage("Gym admin password")
            .StrongPassword();

        RuleFor(v => v.GymAdminPasswordConfirm).Matches(v => v.GymAdminPassword).WithMessage("Password and password confirmation must match.");

        RuleFor(v => v.EscalationEmail)
            .NotEmptyWithMessage("Escalation email")
            .EmailAddress().WithMessage("A valid email address must be provided.");
    }
}

public class CreateGymAdministratorUserCommandHandler : IRequestHandler<CreateGymAdministratorUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CreateGymAdministratorUserCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }
    public async Task<Result> Handle(CreateGymAdministratorUserCommand command, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms.AsNoTracking().FirstOrDefaultAsync(g => g.Id == command.GymId, cancellationToken);

        if (gym == null)
        {
            return Result.Failure(["Gym not found"]);
        }

        var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == command.GymAdminEmail, cancellationToken);

        if (existingUser != null)
        {
            return Result.Failure(["This email is already in use"]);
        }

        var result = await _identityService.CreateGymManagementUserAsync
        (
            command.GymAdminEmail,
            command.GymAdminPassword,
            command.GymAdminFirstName,
            command.GymAdminLastName,
            Roles.GymAdministrator,
            gym,
            command.EscalationEmail
        );

        return result.Result;
    }
}