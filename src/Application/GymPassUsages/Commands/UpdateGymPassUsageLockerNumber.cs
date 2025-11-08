using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.GymPassUsages.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record UpdateGymPassUsageLockerNumberCommand(string GymPassUsageId, string LockerNumber) : IRequest;

public class UpdateGymPassUsageLockerNumberCommandValidator : AbstractValidator<UpdateGymPassUsageLockerNumberCommand>
{
    public UpdateGymPassUsageLockerNumberCommandValidator()
    {
        RuleFor(v => v.GymPassUsageId).NotEmptyWithMessage(nameof(UpdateGymPassUsageLockerNumberCommand.GymPassUsageId));

        RuleFor(v => v.LockerNumber).NotEmptyWithMessage(nameof(UpdateGymPassUsageLockerNumberCommand.LockerNumber));
    }
}

public class UpdateGymPassUsageLockerNumberCommandHandler : IRequestHandler<UpdateGymPassUsageLockerNumberCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateGymPassUsageLockerNumberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task Handle(UpdateGymPassUsageLockerNumberCommand command, CancellationToken cancellationToken)
    {
        var gymPassUsage = await _context.GymPassUsages.FindAsync(command.GymPassUsageId);

        Guard.Against.NotFound(command.GymPassUsageId, gymPassUsage);

        if (gymPassUsage.HasGymSessionEnded())
        {
            throw new BadRequestException("Gym session already ended, you cannot change the locker number after this.");
        }

        gymPassUsage.LockerNumber = command.LockerNumber;

        await _context.SaveChangesAsync();
    }
}
