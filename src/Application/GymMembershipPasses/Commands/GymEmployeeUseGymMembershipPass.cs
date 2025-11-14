using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymMembershipPasses.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GymEmployeeUseGymMembershipPassCommand(string GymMembershipPassId, string LockerNumber) : IRequest<PassUseResult>;

public class GymEmployeeUseGymMembershipPassCommandValidator : AbstractValidator<GymEmployeeUseGymMembershipPassCommand>
{
    public GymEmployeeUseGymMembershipPassCommandValidator()
    {
        RuleFor(v => v.GymMembershipPassId).NotEmptyWithMessage(nameof(GymEmployeeUseGymMembershipPassCommand.GymMembershipPassId));

        RuleFor(v => v.LockerNumber).NotEmptyWithMessage(nameof(GymEmployeeUseGymMembershipPassCommand.LockerNumber));
    }
}

public class GymEmployeeUseGymMembershipPassCommandHandler : IRequestHandler<GymEmployeeUseGymMembershipPassCommand, PassUseResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GymEmployeeUseGymMembershipPassCommandHandler> _logger;

    public GymEmployeeUseGymMembershipPassCommandHandler(IApplicationDbContext context, IUser user, ILogger<GymEmployeeUseGymMembershipPassCommandHandler> logger)
    {
        _context = context;
        _user = user;
        _logger = logger;
    }

    public async Task<PassUseResult> Handle(GymEmployeeUseGymMembershipPassCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.ApplicationUserId != null && ge.ApplicationUserId == _user.Id);

        if (gymEmployment is null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger,
                _user.Roles,
                _user.Id,
                nameof(GymEmployment));

            throw new SystemException(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        var pass = await _context
            .GymMembershipPasses
            .Include(p => p.GymMembership)
            .FirstOrDefaultAsync(p => p.Id == command.GymMembershipPassId);

        Guard.Against.NotFound(command.GymMembershipPassId, pass);

        if (pass.GymMembership.GymId != gymEmployment.GymId)
        {
            throw new ForbiddenAccessException();
        }

        if (pass.GymMembership.Status == GymMembershipStatus.Banned)
        {
            throw new BadRequestException("User is banned from the gym.");
        }

        var passUsage = pass.Use(command.LockerNumber);

        if (passUsage.PassUseResult == PassUseResult.AlreadyHasNoUsesLeft)
        {
            _logger.LogCritical("User request to use an already expired pass.");
        }

        await _context.GymPassUsages.AddAsync(passUsage);
        await _context.SaveChangesAsync();

        return passUsage.PassUseResult;
    }
}
