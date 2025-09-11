using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Extensions;
using FitPass.Domain;
using FitPass.Domain.Constants;

namespace FitPass.Application.Gyms.Commands;

public record RegisterGymCommand(
    string Name,
    string Address,
    string? OwnerName,
    string GymAdminEmail,
    string GymAdminFirstName,
    string GymAdminLastName,
    string GymAdminPassword,
    string GymAdminPasswordConfirm,
    string EscalationEmail
) : IRequest<Result>;

public class RegisterGymCommandValidator : AbstractValidator<RegisterGymCommand>
{
    public RegisterGymCommandValidator()
    {
        RuleFor(v => v.Name).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Gym name");

        RuleFor(v => v.Address).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Address, "Gym address");

        RuleFor(v => v.GymAdminEmail)
            .NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Email, "Email address")
            .EmailAddress().WithMessage("An email address is required for the gym administrator account.");

        RuleFor(v => v.GymAdminFirstName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "First name");

        RuleFor(v => v.GymAdminLastName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Last name");

        RuleFor(v => v.GymAdminPassword)
            .NotEmptyWithMessage("Password")
            .StrongPassword();

        RuleFor(v => v.GymAdminPasswordConfirm)
            .NotEmptyWithMessage("Password confirmation")
            .Equal(v => v.GymAdminPassword).WithMessage("Password and password confirmation must match.");

        RuleFor(v => v.EscalationEmail)
            .NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Email, "Escalation email")
            .EmailAddress().WithMessage("An escalation email address from a higher-level contact than the gym administrator is required.");
    }
}

public class RegisterGymCommandHandler : IRequestHandler<RegisterGymCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public RegisterGymCommandHandler(IIdentityService identityService, IApplicationDbContext context, IQrCodeService qrCodeService)
    {
        _identityService = identityService;
        _context = context;
        _qrCodeService = qrCodeService;
    }

    public async Task<Result> Handle(RegisterGymCommand request, CancellationToken cancellationToken)
    {
        var gymId = Guid.NewGuid().ToString();

        var gym = new Gym
        {
            Id = gymId,
            Name = request.Name,
            Address = request.Address,
            EscalationEmail = request.EscalationEmail,
            QRCode = _qrCodeService.GenerateQrCode(gymId),
            OwnerName = request.OwnerName,
        };

        await _context.Gyms.AddAsync(gym, cancellationToken);
        await _identityService.CreateUserAsync(request.GymAdminEmail, request.GymAdminPassword, request.GymAdminFirstName, request.GymAdminLastName, Roles.GymAdministrator);
    }
}