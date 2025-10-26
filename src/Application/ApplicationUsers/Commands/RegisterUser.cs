using Fitpass.Application.Common.Exceptions;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;

namespace Fitpass.Application.ApplicationUsers.Commands;

public record RegisterUserCommand
    (
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PasswordConfirm
    ) : IRequest<TokenResponse>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(nameof(RegisterUserCommand.FirstName), MaxStringLengths.Name);

        RuleFor(v => v.LastName!).NotEmptyWithMaxLenghtAndMessage(nameof(RegisterUserCommand.LastName), MaxStringLengths.Name);

        RuleFor(v => v.Email).ValidEmailAddress(nameof(RegisterUserCommand.Email));

        RuleFor(v => v.Password).StrongPassword();

        RuleFor(v => v.PasswordConfirm)
            .Equal(v => v.PasswordConfirm)
            .WithMessage(ErrorMessages.PropertyMustEqualToAnotherProperty(nameof(RegisterUserCommand.Password), nameof(RegisterUserCommand.PasswordConfirm)));
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IStripeCustomerService _stripeCustomerService;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterUserCommandHandler(IIdentityService identityService, IApplicationDbContext context, IStripeCustomerService stripeCustomerService, IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _context = context;
        _stripeCustomerService = stripeCustomerService;
        _jwtTokenService = jwtTokenService;
    }
    public async Task<TokenResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
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
            GymStaffAssignment = null,
            PaymentProfile = null //created in .CreateCustomer
        };

        var result = await _identityService.CreateUserAsync(user, command.Password, cancellationToken);

        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description).ToList()));
        }

        /*
        await _stripeCustomerService.CreateCustomer(user);

        user.AddDomainEvent(new UserRegisteredEvent(user));
        */

        await _context.SaveChangesAsync();

        var jwtResponse = await _jwtTokenService.GenerateTokenAsync(user, cancellationToken);

        return jwtResponse;
    }
}
