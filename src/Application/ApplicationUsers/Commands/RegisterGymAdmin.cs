using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Events.Users;

namespace FitPass.Application.ApplicationUsers.Commands;

public record RegisterGymAdminCommand
    (
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PasswordConfirm
    ): IRequest<(Result result, string? jwtToken)>;

public class RegisterGymAdminCommandValidator : AbstractValidator<RegisterGymAdminCommand>
{
    public RegisterGymAdminCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "First name");

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Last name");

        RuleFor(v => v.Email).EmailAddress().WithMessage("A valid email address is required");

        RuleFor(v => v.Password).StrongPassword();

        RuleFor(v => v.PasswordConfirm).Equal(v => v.PasswordConfirm);
    }
}

public class RegisterGymAdminCommandHandler : IRequestHandler<RegisterGymAdminCommand, (Result result, string? jwtToken)>
{
    private readonly IIdentityService _identityService;
    private readonly IStripeCustomerService _stripeCustomerService;
    private readonly IApplicationDbContext _context;

    public RegisterGymAdminCommandHandler(IIdentityService identityService, IStripeCustomerService stripeCustomerService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _stripeCustomerService = stripeCustomerService;
        _context = context;
    }
    public async Task<(Result result, string? jwtToken)> Handle(RegisterGymAdminCommand command, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            UserGymMemberships = null,
            PaymentProfile = null,
            GymStaffAssigment = null
        };

        user.GymStaffAssigment = new GymStaffAssigment
        {
            ApplicationUserId = user.Id,
            GymId = null,
            Role = Roles.GymAdministrator
        };

        var result = await _identityService.CreateUserAsync(user, command.Password, cancellationToken);

        if (!result.Succeeded)
        {
            return (Result.Failure([..result.Errors.Select(e => e.Description).ToList()]), null);
        }

        var jwtToken = await _identityService.GenerateJWTTokenAsync(user, cancellationToken);

        user.AddDomainEvent(new GymAdminRegisteredEvent(user));

        return (Result.Success(), jwtToken);
    }
}
