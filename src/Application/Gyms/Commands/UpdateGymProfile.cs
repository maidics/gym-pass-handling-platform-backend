using Fitpass.Application.Gyms.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace Fitpass.Application.Gyms.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateGymProfileCommand(
    string GymId,
    string GymName,
    string GymAddress,
    string? OwnerName
) : IRequest<GymDto?>;

public class UpdateGymProfileCommandValidator : AbstractValidator<UpdateGymProfileCommand>
{
    public UpdateGymProfileCommandValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage("Gym id");

        RuleFor(v => v.GymName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Gym name");

        RuleFor(v => v.GymAddress).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Address, "Gym address");
    }
}

public class UpdateGymProfileCommandHandler : IRequestHandler<UpdateGymProfileCommand, GymDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateGymProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public Task<GymDto?> Handle(UpdateGymProfileCommand request, CancellationToken cancellationToken)
    {
        
    }
}