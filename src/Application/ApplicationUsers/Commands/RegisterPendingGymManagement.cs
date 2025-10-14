using Fitpass.Application.Common.Exceptions;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Events.Users;

namespace FitPass.Application.ApplicationUsers.Commands;

public record RegisterPendingGymManagementCommand
    (
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PasswordConfirm
    ): IRequest<TokenResponse>;

public class RegisterPendingGymManagementCommandValidator : AbstractValidator<RegisterPendingGymManagementCommand>
{
    public RegisterPendingGymManagementCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "First name");

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Last name");

        RuleFor(v => v.Email).EmailAddress().WithMessage("A valid email address is required");

        RuleFor(v => v.Password).StrongPassword();

        RuleFor(v => v.PasswordConfirm).Equal(v => v.PasswordConfirm);
    }
}

public class RegisterPendingGymManagementCommandHandler : IRequestHandler<RegisterPendingGymManagementCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;

    public RegisterPendingGymManagementCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task<TokenResponse> Handle(RegisterPendingGymManagementCommand command, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            UserName = command.Email,
            UserGymMemberships = null,
            PaymentProfile = null,
            GymStaffAssignment = null
        };

        user.GymStaffAssignment = new GymStaffAssignment
        {
            ApplicationUserId = user.Id,
            GymId = null,
            Role = Roles.PendingGymManagement
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
