using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;
using FitPass.Domain.Events.Gyms;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record UpdateGymStatusCommand(string GymId, GymStatus NewGymStatus, string Rationale)
    : IRequest<Result>;

public class UpdateGymStatusCommandValidator : AbstractValidator<UpdateGymStatusCommand>
{
    public UpdateGymStatusCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(
                localizer,
                nameof(SharedResource.Id),
                nameof(SharedResource.Gym)
            );

        RuleFor(v => v.NewGymStatus)
            .NotEmpty()
            .WithMessage(localizer.GetNewValueIsRequired(nameof(SharedResource.GymStatus)));

        RuleFor(v => v.Rationale)
            .NotEmptyWithMaxLengthAndMessageLocalized(
                localizer,
                nameof(SharedResource.Rationale),
                MaxLengths.Description
            );
    }
}

public class UpdateGymStatusCommandHandler : IRequestHandler<UpdateGymStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;

    public UpdateGymStatusCommandHandler(IApplicationDbContext context, ILocalizer localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(
        UpdateGymStatusCommand command,
        CancellationToken cancellationToken
    )
    {
        var gym = await _context.Gyms.FindAsync([command.GymId], cancellationToken);

        if (gym is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Gym)));
        }

        if (gym.Status == command.NewGymStatus)
        {
            return Result.Success();
        }

        gym.Status = command.NewGymStatus;

        //rationale not saved to db here
        gym.AddDomainEvent(
            new GymStatusUpdatedByAppAdminEvent(
                gym.Id,
                command.NewGymStatus,
                command.Rationale,
                gym.Name
            )
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
