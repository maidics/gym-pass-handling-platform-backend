using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;

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
        When(v => string.IsNullOrEmpty(v.NonRegisteredUserEmail), () =>
        {
            RuleFor(v => v.NonRegisteredUserPhoneNumber)
                .NotNull()
                .WithMessage(ErrorMessages.PropertyCannotBeNullIfAnotherIsNull(nameof(RegisterNonRegisteredUserCommand.NonRegisteredUserPhoneNumber), nameof(RegisterNonRegisteredUserCommand.NonRegisteredUserEmail)));
        });

        When(v => string.IsNullOrEmpty(v.NonRegisteredUserPhoneNumber), () =>
        {
            RuleFor(v => v.NonRegisteredUserEmail)
                .NotNull()
                .WithMessage(ErrorMessages.PropertyCannotBeNullIfAnotherIsNull(nameof(RegisterNonRegisteredUserCommand.NonRegisteredUserEmail), nameof(RegisterNonRegisteredUserCommand.NonRegisteredUserPhoneNumber)));
        });

        When(v => !string.IsNullOrEmpty(v.NonRegisteredUserEmail), () =>
        {
            RuleFor(v => v.NonRegisteredUserEmail!).NotEmptyWithMaxLenghtAndMessage(nameof(RegisterNonRegisteredUserCommand.NonRegisteredUserEmail), MaxStringLengths.Email);
        });

        When(v => !string.IsNullOrEmpty(v.NonRegisteredUserPhoneNumber), () =>
        {
            RuleFor(v => v.NonRegisteredUserEmail!).NotEmptyWithMaxLenghtAndMessage(nameof(RegisterNonRegisteredUserCommand.NonRegisteredUserEmail), MaxStringLengths.PhoneNumber);
        });

        RuleFor(v => v.Password).StrongPassword();

        RuleFor(v => v.PasswordConfirm)
            .NotEmptyWithMessage(nameof(RegisterNonRegisteredUserCommand.PasswordConfirm))
            .Equal(v => v.Password)
            .WithMessage(ErrorMessages.PropertyMustEqualToAnotherProperty(nameof(RegisterNonRegisteredUserCommand.Password), nameof(RegisterNonRegisteredUserCommand.PasswordConfirm)));
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

        var applicationUserId = Guid.CreateVersion7().ToString();

        var ugms = nonRegisteredUser.UserGymMemberships;

        foreach (var ugm in ugms)
        {
            ugm.ApplicationUserId = applicationUserId;
            ugm.NonRegisteredUserId = null;
        }

        nonRegisteredUser.PaymentProfile!.ApplicationUserId = applicationUserId;
        nonRegisteredUser.PaymentProfile!.NonRegisteredUserId = null;

        var applicationUser = new ApplicationUser
        { 
            Id = applicationUserId,
            FirstName = nonRegisteredUser.FirstName,
            LastName = nonRegisteredUser.LastName,
            UserGymMemberships = [],
            GymStaffAssignment = null,
            Email = nonRegisteredUser.Email,
            PhoneNumber = nonRegisteredUser.PhoneNumber,
            PaymentProfile = nonRegisteredUser.PaymentProfile,
        };

        var result = await _identityService.CreateUserAsync(applicationUser, command.Password, cancellationToken);

        if (!result.Succeeded)
        {
            throw new Exception($"An error occured during registration: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        _context.NonRegisteredUsers.Remove(nonRegisteredUser);

        await _context.SaveChangesAsync();
    }
}
