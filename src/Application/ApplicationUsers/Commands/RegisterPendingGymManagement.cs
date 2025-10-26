using Fitpass.Application.Common.Exceptions;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Events.Users;
using FitPass.Domain.Strings;

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
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(nameof(RegisterPendingGymManagementCommand.FirstName), MaxStringLengths.Name);

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessage(nameof(RegisterPendingGymManagementCommand.LastName), MaxStringLengths.Name);

        RuleFor(v => v.Email).ValidEmailAddress(nameof(RegisterPendingGymManagementCommand.Email));

        RuleFor(v => v.Password).StrongPassword();

        RuleFor(v => v.PasswordConfirm)
            .Equal(v => v.PasswordConfirm)
            .WithMessage(ErrorMessages.PropertyMustEqualToAnotherProperty(nameof(RegisterPendingGymManagementCommand.Password), nameof(RegisterPendingGymManagementCommand.PasswordConfirm)));
    }
}

public class RegisterPendingGymManagementCommandHandler : IRequestHandler<RegisterPendingGymManagementCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterPendingGymManagementCommandHandler(IIdentityService identityService, IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
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

        var jwtToken = await _jwtTokenService.GenerateTokenAsync(user, cancellationToken);

        user.AddDomainEvent(new PendingGymAdminRegisteredEvent(user));

        return jwtToken;
    }
}
