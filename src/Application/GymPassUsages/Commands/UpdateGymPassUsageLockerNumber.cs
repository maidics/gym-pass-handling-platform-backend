using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Infrastructure.Localization.Resources;

namespace FitPass.Application.GymPassUsages.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record UpdateGymPassUsageLockerNumberCommand(string GymPassUsageId, string LockerNumber) : IRequest<Result>;

public class UpdateGymPassUsageLockerNumberCommandValidator : AbstractValidator<UpdateGymPassUsageLockerNumberCommand>
{
    public UpdateGymPassUsageLockerNumberCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymPassUsageId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.GymPassUsage));

        RuleFor(v => v.LockerNumber)
            .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.LockerNumber));
    }
}

public class UpdateGymPassUsageLockerNumberCommandHandler : IRequestHandler<UpdateGymPassUsageLockerNumberCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;

    public UpdateGymPassUsageLockerNumberCommandHandler(
        IApplicationDbContext context,
        ILocalizer localizer)
    {
        _context = context;
        _localizer = localizer;
    }
    public async Task<Result> Handle(UpdateGymPassUsageLockerNumberCommand command, CancellationToken cancellationToken)
    {
        var gymPassUsage = await _context.GymPassUsages.FindAsync(command.GymPassUsageId);

        if (gymPassUsage is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymPassUsage)));
        }

        if (gymPassUsage.HasGymSessionEnded())
        {
            return Result.BusinessRuleViolation(_localizer.Get(
                nameof(SharedResource.NoChangingLockerNumberAfterGymSessionEnded)));
        }

        gymPassUsage.LockerNumber = command.LockerNumber;

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
