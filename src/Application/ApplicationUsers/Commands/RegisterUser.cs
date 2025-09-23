using Fitpass.Application.Common.Exceptions;
using Fitpass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace Fitpass.Application.ApplicationUsers.Commands;

public record RegisterUserCommand
    (
        string FirstName,
        string? LastName,
        string Email,
        string Password,
        string PasswordConfirm
    ) : IRequest<string>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "First name");

        When(v => !string.IsNullOrEmpty(v.LastName), () =>
        {
            RuleFor(v => v.LastName!).MaxLengthWithMessage(MaxStringLengths.Name, "Last name");
        });

        RuleFor(v => v.Email).EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(v => v.Password).StrongPassword();

        RuleFor(v => v.PasswordConfirm).Equal(v => v.PasswordConfirm);
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
{
    private readonly IIdentityService _identityService;

    public RegisterUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task<string> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            UserGymMemberships = null,
            GymStaffAssigment = null
        };

        var result = await _identityService.CreateUserAsync(user, command.Password, cancellationToken);

        if (result.IsDuplicateEmail())
        {
            throw new ConflictException("Email is already in use.");
        }
        else if (!result.Succeeded)
        {
            throw new ValidationException(string.Join(", ", result.Errors.Select(e => e.Description).ToList()));
        }

        var jwtToken = await _identityService.GenerateJWTTokenAsync(user, cancellationToken);

        return jwtToken;
    }
}