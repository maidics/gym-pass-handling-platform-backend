using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.GymPassUsages.Commands;

[Authorize (Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GymEmployeeEndUserGymSessionCommand(string GymPassUsageId) : IRequest;

public class GymEmployeeEndUserGymSessionCommandValidator : AbstractValidator<GymEmployeeEndUserGymSessionCommand>
{
    public GymEmployeeEndUserGymSessionCommandValidator()
    {
        RuleFor(v => v.GymPassUsageId).NotEmptyWithMessage(nameof(GymEmployeeEndUserGymSessionCommand.GymPassUsageId));
    }
}

public class GymEmployeeEndUserGymSessionCommandHandler : IRequestHandler<GymEmployeeEndUserGymSessionCommand>
{
    private readonly IApplicationDbContext _context;

    public GymEmployeeEndUserGymSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(GymEmployeeEndUserGymSessionCommand command, CancellationToken cancellationToken)
    {
        var gymPassUsage = await _context.GymPassUsages.FindAsync(command.GymPassUsageId);

        Guard.Against.NotFound(command.GymPassUsageId, gymPassUsage);

        gymPassUsage.FinishGymSession();

        await _context.SaveChangesAsync();
    }
}
