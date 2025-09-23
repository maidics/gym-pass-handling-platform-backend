using Fitpass.Application.Gyms.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace Fitpass.Application.Gyms.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateMyGymProfileCommand(
    string GymId,
    string GymName,
    string GymAddress,
    GymTier GymTier,
    string? GymOwnerName
) : IRequest<GymDto>;

public class UpdateMyGymProfileCommandValidator : AbstractValidator<UpdateMyGymProfileCommand>
{
    public UpdateMyGymProfileCommandValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage("Gym id");

        RuleFor(v => v.GymName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Gym name");

        RuleFor(v => v.GymAddress).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Address, "Gym address");
    }
}

public class UpdateMyGymProfileCommandHandler : IRequestHandler<UpdateMyGymProfileCommand, GymDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateMyGymProfileCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<GymDto> Handle(UpdateMyGymProfileCommand command, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms.FindAsync(command.GymId, cancellationToken);

        Guard.Against.NotFound(command.GymId, gym, "Id");

        gym.Name = command.GymName;
        gym.Address = command.GymAddress;
        gym.Tier = command.GymTier;
        gym.OwnerName = command.GymOwnerName;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<GymDto>(gym);
    }
}