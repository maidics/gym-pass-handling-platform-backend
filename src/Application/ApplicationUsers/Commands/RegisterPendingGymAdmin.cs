using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Events.Users;

namespace FitPass.Application.ApplicationUsers.Commands;

public record RegisterPendingGymAdmin
    (
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PasswordConfirm
    ): IRequest<string>;

public class RegisterPendingGymAdminValidator : AbstractValidator<RegisterPendingGymAdmin>
{
    public RegisterPendingGymAdminValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "First name");

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Last name");

        RuleFor(v => v.Email).EmailAddress().WithMessage("A valid email address is required");

        RuleFor(v => v.Password).StrongPassword();

        RuleFor(v => v.PasswordConfirm).Equal(v => v.PasswordConfirm);
    }
}

public class RegisterPendingGymAdminHandler : IRequestHandler<RegisterPendingGymAdmin, string>
{
    private readonly IIdentityService _identityService;

    public RegisterPendingGymAdminHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task<string> Handle(RegisterPendingGymAdmin command, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            UserName = command.Email,
            UserGymMemberships = null,
            PaymentProfile = null,
            GymStaffAssigment = null
        };

        user.GymStaffAssigment = new GymStaffAssigment
        {
            ApplicationUserId = user.Id,
            GymId = null,
            Role = Roles.PendingGymAdministrator
        };

        var result = await _identityService.CreateUserAsync(user, command.Password, cancellationToken);

        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description).ToList()));
        }

        var roleResult = await _identityService.AddToRoleAsync(user, Roles.GymAdministrator);

        if (!roleResult.Succeeded)
        {
            throw new BadRequestException(string.Join(", ", roleResult.Errors));
        }

        var jwtToken = await _identityService.GenerateJWTTokenAsync(user, cancellationToken);

        user.AddDomainEvent(new PendingGymAdminRegisteredEvent(user));

        return jwtToken;
    }
}
