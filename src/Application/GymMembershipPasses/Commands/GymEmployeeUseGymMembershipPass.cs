using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymMembershipPasses.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
//Take user id from the qr code as well, if this request does not come from qr code we can do a check to make it more safe
public record GymEmployeeUseGymMembershipPassCommand(string GymMembershipPassId, string UserId, string LockerNumber) : IRequest<Result<PassUseResult>>;

public class GymEmployeeUseGymMembershipPassCommandValidator : AbstractValidator<GymEmployeeUseGymMembershipPassCommand>
{
    public GymEmployeeUseGymMembershipPassCommandValidator()
    {
        RuleFor(v => v.GymMembershipPassId).NotEmptyWithMessage(nameof(GymEmployeeUseGymMembershipPassCommand.GymMembershipPassId));

        RuleFor(v => v.LockerNumber).NotEmptyWithMessage(nameof(GymEmployeeUseGymMembershipPassCommand.LockerNumber));
    }
}

public class GymEmployeeUseGymMembershipPassCommandHandler : IRequestHandler<GymEmployeeUseGymMembershipPassCommand, Result<PassUseResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GymEmployeeUseGymMembershipPassCommandHandler> _logger;
    private readonly TimeProvider _timeProvider;

    public GymEmployeeUseGymMembershipPassCommandHandler(
        IApplicationDbContext context, 
        IUser user, 
        ILogger<GymEmployeeUseGymMembershipPassCommandHandler> logger,
        TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<Result<PassUseResult>> Handle(GymEmployeeUseGymMembershipPassCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId != null && ge.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var pass = await _context
            .GymMembershipPasses
            .Include(p => p.GymMembership)
            .FirstOrDefaultAsync(p => p.Id == command.GymMembershipPassId);

        if (pass is null)
        {
            return Result.NotFound(nameof(GymMembershipPass));
        }

        if (pass.UserId != command.UserId)
        {
            return Result.Forbidden("This pass does not belong to user.");
        }

        if (pass.GymMembership.GymId != gymEmployment.GymId)
        {
            return Result.Forbidden();
        }

        if (pass.GymMembership.Status == GymMembershipStatus.Banned)
        {
            return Result.BusinessRuleViolation("User is banned from the gym.");
        }

        var passUsage = pass.Use(command.LockerNumber, _timeProvider.GetUtcNow());

        if (passUsage.PassUseResult == PassUseResult.Expired)
        {
            _logger.LogError("User request to use an already expired pass.");
        }

        await _context.GymPassUsages.AddAsync(passUsage);
        await _context.SaveChangesAsync();

        return Result.Success(passUsage.PassUseResult);
    }
}
