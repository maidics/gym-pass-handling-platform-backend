using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Events.Users;

namespace Fitpass.Application.ApplicationUsers.Commands;

public record RegisterUserCommand
    (
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PasswordConfirm
    ) : IRequest<string>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "First name");

        RuleFor(v => v.LastName!).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Last name");

        RuleFor(v => v.Email).EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(v => v.Password).StrongPassword();

        RuleFor(v => v.PasswordConfirm).Equal(v => v.PasswordConfirm);
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public RegisterUserCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }
    public async Task<string> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var existingUser = await _context
            .ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(au => au.Email == command.Email);

        if (existingUser != null)
        {
            throw new ConflictException("User with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            UserGymMemberships = null,
            GymStaffAssigment = null
        };

        var result = await _identityService.CreateUserAsync(user, command.Password, cancellationToken);

        if (!result.Succeeded)
        {
            throw new ValidationException(string.Join(", ", result.Errors.Select(e => e.Description).ToList()));
        }

        user.AddDomainEvent(new UserRegisteredEvent(user));

        await _context.SaveChangesAsync();

        var jwtToken = await _identityService.GenerateJWTTokenAsync(user, cancellationToken);

        return jwtToken;
    }
}
