using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.NonRegisteredUsers.Commands;

public record RegisterNonRegisteredUserCommand
(
    string? NonRegisteredUserEmail,
    string? NonRegisteredUserPhoneNumber,
    string Password,
    string PasswordConfirm
) : IRequest;

public class RegisterNonRegisteredUserCommandValidator : AbstractValidator<RegisterNonRegisteredUserCommand>
{
    public RegisterNonRegisteredUserCommandValidator()
    {
        RuleFor(v => v).Must(v => !string.IsNullOrEmpty(v.NonRegisteredUserEmail) || !string.IsNullOrEmpty(v.NonRegisteredUserPhoneNumber));

        When(v => !string.IsNullOrEmpty(v.NonRegisteredUserEmail), () =>
        {
            RuleFor(v => v.NonRegisteredUserEmail!).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Email, "Non registered user email");
        });

        When(v => !string.IsNullOrEmpty(v.NonRegisteredUserPhoneNumber), () =>
        {
            RuleFor(v => v.NonRegisteredUserEmail!).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.PhoneNumber, "Non registered phone number");
        });

        RuleFor(v => v.Password)
            .NotEmptyWithMessage("Password")
            .StrongPassword();

        RuleFor(v => v.PasswordConfirm)
            .NotEmptyWithMessage("Password confirmation")
            .Equal(v => v.Password).WithMessage("Your password and password confirmation must match.");
    }
}

public class RegisterNonRegisteredUserCommandHandler : IRequestHandler<RegisterNonRegisteredUserCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public RegisterNonRegisteredUserCommandHandler(IIdentityService identityService, IApplicationDbContext context, IUser user)
    {
        _identityService = identityService;
        _context = context;
        _user = user;
    }

    public async Task Handle(RegisterNonRegisteredUserCommand command, CancellationToken cancellationToken)
    {
        if (_user.Id != null)
        {
            throw new UnauthorizedAccessException("Please log out for this action.");
        }

        var nonRegisteredUser = command.NonRegisteredUserEmail != null ?
            await _context.NonRegisteredUsers.FirstOrDefaultAsync(nru => nru.Email == command.NonRegisteredUserEmail, cancellationToken) :
            await _context.NonRegisteredUsers.FirstOrDefaultAsync(nru => nru.PhoneNumber == command.NonRegisteredUserPhoneNumber, cancellationToken);

        Guard.Against.NotFound("Email & PhoneNumber", nonRegisteredUser, "Email or PhoneNumber");

        var applicationUser = new ApplicationUser
        {
            FirstName = nonRegisteredUser.FirstName,
            LastName = nonRegisteredUser.LastName,
            UserGymMemberships = [],
            GymStaffAssigment = null,
            Email = nonRegisteredUser.Email,
            PhoneNumber = nonRegisteredUser.PhoneNumber
        };

        var result = await _identityService.CreateUserAsync(applicationUser, command.Password, cancellationToken);

        if (!result.Succeeded)
        {
            throw new Exception($"An error occured during registration: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}