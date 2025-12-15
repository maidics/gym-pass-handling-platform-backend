using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymPassUsages.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record UpdateGymPassUsageLockerNumberCommand(string GymPassUsageId, string LockerNumber) : IRequest<Result>;

public class UpdateGymPassUsageLockerNumberCommandValidator : AbstractValidator<UpdateGymPassUsageLockerNumberCommand>
{
    public UpdateGymPassUsageLockerNumberCommandValidator()
    {
        RuleFor(v => v.GymPassUsageId).NotEmptyLocalized(nameof(UpdateGymPassUsageLockerNumberCommand.GymPassUsageId));

        RuleFor(v => v.LockerNumber).NotEmptyLocalized(nameof(UpdateGymPassUsageLockerNumberCommand.LockerNumber));
    }
}

public class UpdateGymPassUsageLockerNumberCommandHandler : IRequestHandler<UpdateGymPassUsageLockerNumberCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateGymPassUsageLockerNumberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Result> Handle(UpdateGymPassUsageLockerNumberCommand command, CancellationToken cancellationToken)
    {
        var gymPassUsage = await _context.GymPassUsages.FindAsync(command.GymPassUsageId);

        if (gymPassUsage is null)
        {
            return Result.NotFound(nameof(GymPassUsage));
        }

        if (gymPassUsage.HasGymSessionEnded())
        {
            return Result.BusinessRuleViolation("Gym session already ended, you cannot change the locker number after this.");
        }

        gymPassUsage.LockerNumber = command.LockerNumber;

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
