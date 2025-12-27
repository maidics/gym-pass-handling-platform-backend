using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.GymPassUsages.Commands;

[Authorize (Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record EndUserGymSessionCommand(string GymPassUsageId) : IRequest<Result>;

public class EndUserGymSessionCommandValidator : AbstractValidator<EndUserGymSessionCommand>
{
    public EndUserGymSessionCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymPassUsageId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.GymPassUsage));
    }
}

public class EndUserGymSessionCommandHandler : IRequestHandler<EndUserGymSessionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ILocalizer _localizer;

    public EndUserGymSessionCommandHandler(
        IApplicationDbContext context,
        TimeProvider timeProvider,
        ILocalizer localizer)
    {
        _context = context;
        _timeProvider = timeProvider;
        _localizer = localizer;
    }

    public async Task<Result> Handle(EndUserGymSessionCommand command, CancellationToken cancellationToken)
    {
        var gymPassUsage = await _context.GymPassUsages.FindAsync(command.GymPassUsageId);

        if (gymPassUsage is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymPassUsage)));
        }

        gymPassUsage.EndGymSession(_timeProvider.GetUtcNow());

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
