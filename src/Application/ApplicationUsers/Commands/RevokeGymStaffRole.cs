using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace FitPass.Application.ApplicationUsers.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public  record RevokeGymStaffRoleCommand(string GymStaffMemberId, string? Message) : IRequest;

public class RevokeGymStaffRoleCommandValidator : AbstractValidator<RevokeGymStaffRoleCommand>
{
    public RevokeGymStaffRoleCommandValidator()
    {
        RuleFor(v => v.GymStaffMemberId).NotEmptyWithMessage("Gym staff member id");

        When(v => v.Message != string.Empty || v.Message != null, () =>
        {
            RuleFor(v => v.Message!).MaxLengthWithMessage(MaxStringLengths.Description, "Message");
        });
    }
}

public class RevokeGymStaffRoleCommandHandler : IRequestHandler<RevokeGymStaffRoleCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;

    public RevokeGymStaffRoleCommandHandler(IIdentityService identityService, IUser user, IApplicationDbContext context)
    {
        _identityService = identityService;
        _user = user;
        _context = context;
    }

    public async Task Handle(RevokeGymStaffRoleCommand command, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(command.GymStaffMemberId);

        Guard.Against.NotFound(command.GymStaffMemberId, user, "Gym staff member");

        var roleCheckResult = await _identityService.IsInRoleAsync(user, Roles.GymStaff);

        if (!roleCheckResult)
        {
            throw new Common.Exceptions.ValidationException("User role", "User is not in gym staff role.");
        }

        var gymStaffAssigment = await _context.GymStaffAssigments.AsNoTracking().FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id!);

        if (user.GymStaffAssignment!.GymId != gymStaffAssigment!.GymId)
        {
            throw new ForbiddenAccessException();
        }

        var roleRevocationResult = await _identityService.RemoveFromRoleAsync(user, Roles.GymStaff);

        if (!roleRevocationResult.Succeeded)
        {
            throw new Common.Exceptions.ValidationException("User role", $"Failed to remove user from gym staff role: {string.Join(", ", roleRevocationResult.Errors)}");
        }
    }
}
