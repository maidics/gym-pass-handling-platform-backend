using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize(
    Roles = $"{Roles.GymAdministrator},{Roles.GymStaff},{Roles.PendingGymEmployee},{Roles.User}"
)]
public record CancelMyRequestCommand(string RequestId) : IRequest<Result>;

public class CancelMyRequestCommandValidator : AbstractValidator<CancelMyRequestCommand>
{
    public CancelMyRequestCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.RequestId)
            .NotEmpty()
            .WithMessage(
                localizer.GetPropertyOfEntityIsRequired(
                    nameof(SharedResource.Id),
                    nameof(SharedResource.Request)
                )
            );
    }
}

public class CancelMyRequestCommandHandler : IRequestHandler<CancelMyRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public CancelMyRequestCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILocalizer localizer
    )
    {
        _context = context;
        _user = user;
        _localizer = localizer;
    }

    public async Task<Result> Handle(
        CancelMyRequestCommand command,
        CancellationToken cancellationToken
    )
    {
        var request = await _context.Requests.FirstOrDefaultAsync(
            x => x.Id == command.RequestId && x.CreatedBy == _user.Id,
            cancellationToken
        );

        if (request is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Request)));
        }

        if (request.Status != RequestStatus.Submitted)
        {
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.RequestIsNotOpen))
            );
        }

        request.Status = RequestStatus.Cancelled;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
