using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMembershipPasses.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
//Take user id from the qr code as well, if this request does not come from qr code we can do a check to make it more safe
public record GymEmployeeUseGymMembershipPassCommand(
    string GymMembershipPassId,
    string UserId,
    string LockerNumber
) : IRequest<Result<PassUseResult>>;

public class GymEmployeeUseGymMembershipPassCommandValidator
    : AbstractValidator<GymEmployeeUseGymMembershipPassCommand>
{
    public GymEmployeeUseGymMembershipPassCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymMembershipPassId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(
                localizer,
                nameof(SharedResource.Id),
                nameof(SharedResource.GymMembershipPass)
            );

        RuleFor(v => v.LockerNumber)
            .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.LockerNumber));
    }
}

public class GymEmployeeUseGymMembershipPassCommandHandler
    : IRequestHandler<GymEmployeeUseGymMembershipPassCommand, Result<PassUseResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;
    private readonly TimeProvider _timeProvider;

    public GymEmployeeUseGymMembershipPassCommandHandler(
        ILocalizer localizer,
        IApplicationDbContext context,
        IUser user,
        TimeProvider timeProvider
    )
    {
        _localizer = localizer;
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task<Result<PassUseResult>> Handle(
        GymEmployeeUseGymMembershipPassCommand command,
        CancellationToken cancellationToken
    )
    {
        var gymEmployment = await _context
            .GymEmployments.AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(
            gymEmployment,
            nameof(GymEmployment),
            _user.Id
        );

        var pass = await _context
            .GymMembershipPasses.Include(p => p.GymMembership)
            .FirstOrDefaultAsync(
                p => p.Id == command.GymMembershipPassId && p.UserId == command.UserId,
                cancellationToken
            );

        if (pass is null)
        {
            return Result.NotFound(
                _localizer.GetNotFound(nameof(SharedResource.GymMembershipPass))
            );
        }

        if (pass.GymMembership.GymId != gymEmployment.GymId)
        {
            return Result.Forbidden(_localizer.Get(nameof(SharedResource.PassIsForAnotherGym)));
        }

        if (pass.GymMembership.Status == GymMembershipStatus.Banned)
        {
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.GymMembershipIsBannedFromTheGym))
            );
        }

        var utcNow = _timeProvider.GetUtcNow();

        if (!pass.IsValid(utcNow))
        {
            var key =
                pass.Type == PassType.Unlimited
                    ? nameof(SharedResource.PassIsExpired)
                    : nameof(SharedResource.PassHasNoUsesLeft);

            return Result.BusinessRuleViolation(_localizer.Get(key));
        }

        var passUsage = pass.Use(gymEmployment.GymId, command.LockerNumber, utcNow);

        await _context.GymPassUsages.AddAsync(passUsage, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(passUsage.PassUseResult);
    }
}
