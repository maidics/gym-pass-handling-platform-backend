using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.GymPassUsages.Commands;

[Authorize (Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record EndUserGymSessionCommand(string GymPassUsageId) : IRequest;

public class EndUserGymSessionCommandValidator : AbstractValidator<EndUserGymSessionCommand>
{
    public EndUserGymSessionCommandValidator()
    {
        RuleFor(v => v.GymPassUsageId).NotEmptyWithMessage(nameof(EndUserGymSessionCommand.GymPassUsageId));
    }
}

public class EndUserGymSessionCommandHandler : IRequestHandler<EndUserGymSessionCommand>
{
    private readonly IApplicationDbContext _context;

    public EndUserGymSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(EndUserGymSessionCommand command, CancellationToken cancellationToken)
    {
        var gymPassUsage = await _context.GymPassUsages.FindAsync(command.GymPassUsageId);

        Guard.Against.NotFound(command.GymPassUsageId, gymPassUsage);

        gymPassUsage.FinishGymSession();

        await _context.SaveChangesAsync();
    }
}
