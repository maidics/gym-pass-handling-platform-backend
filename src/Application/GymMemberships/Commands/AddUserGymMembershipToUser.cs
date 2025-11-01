using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymMemberships.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record AddUserGymMembershipToUserCommand(string UserId) : IRequest;

public class AddUserGymMembershipToUserCommandValidator : AbstractValidator<AddUserGymMembershipToUserCommand>
{
    public AddUserGymMembershipToUserCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmptyWithMessage(nameof(AddUserGymMembershipToUserCommand.UserId));
    }
}

public class AddUserGymMembershipToUserCommandHandler : IRequestHandler<AddUserGymMembershipToUserCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<AddUserGymMembershipToUserCommandHandler> _logger;
    private readonly IIdentityService _identityService;

    public AddUserGymMembershipToUserCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<AddUserGymMembershipToUserCommandHandler> logger,
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _identityService = identityService;
    }
    
    public async Task Handle(AddUserGymMembershipToUserCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.ApplicationUserId == _user.Id);

        if (gymEmployment == null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, nameof(GymEmployment));
            throw new Exception(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        if (!await _identityService.DoesUserExist(command.UserId))
        {
            throw new NotFoundException(command.UserId, "User");
        }

        if (!await _identityService.IsInRoleAsync(command.UserId, Roles.User))
        {
            throw new UnauthorizedAccessException();
        }

        var gymMembership = new GymMembership
        {
            ApplicationUserId = command.UserId,
            GymId = gymEmployment.GymId!
        };

        await _context.GymMemberships.AddAsync(gymMembership);
        await _context.SaveChangesAsync();
    }
}
