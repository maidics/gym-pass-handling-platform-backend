using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Extensions;

namespace FitPass.Application.Gyms.Commands;

public record RegisterGymCommand(
    string Name,
    string Location,
    string? OwnerName,
    string gymAdminEmail,
    string gymAdminName,
    string gymAdminPassword,
    string gymAdminPasswordConfirm
) : IRequest<Result>;

public class RegisterGymCommandValidator : AbstractValidator<RegisterGymCommand>
{
    public RegisterGymCommandValidator()
    {
        RuleFor(v => v.Name).NotEmptyWithMessage("Gym name must be provided.");

        RuleFor(v => v.Location).NotEmptyWithMessage("Gym location must be provided.");

        RuleFor(v => v.gymAdminEmail).EmailAddress().WithMessage("An email address must be provided for the gym administrator account.");

        RuleFor(v => v.gymAdminPassword)
            .NotEmptyWithMessage("An email address must be provided for the gym administrator account.")
            .MinimumLength(10)
            .
    }
}

public class RegisterGymCommandHandler : IRequestHandler<RegisterGymCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public RegisterGymCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<Result> Handle(RegisterGymCommand request, CancellationToken cancellationToken)
    {

    }
}