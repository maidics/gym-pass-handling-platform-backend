using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.NonRegisteredUsers.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.NonRegisteredUsers.Commands;
[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record AddUserGymMembershipToNonRegisteredUserCommand(
    string NonRegisteredUserId
) : IRequest<NonRegisteredUserDto?>;

public class AddUserGymMembershipToNonRegisteredUserCommandValidator : AbstractValidator<AddUserGymMembershipToNonRegisteredUserCommand>
{
    public AddUserGymMembershipToNonRegisteredUserCommandValidator()
    {
        RuleFor(v => v.NonRegisteredUserId).NotEmptyWithMessage("Non registered user id");
    }
}


public class AddUserGymMembershipToNonRegisteredUserCommandHandler : IRequestHandler<AddUserGymMembershipToNonRegisteredUserCommand, NonRegisteredUserDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IUserProfileService _userProfileService;
    private readonly IMapper _mapper;

    public AddUserGymMembershipToNonRegisteredUserCommandHandler(IApplicationDbContext context, IUser user, IUserProfileService userProfileService, IMapper mapper)
    {
        _context = context;
        _user = user;
        _userProfileService = userProfileService;
        _mapper = mapper;
    }
    public async Task<NonRegisteredUserDto?> Handle(AddUserGymMembershipToNonRegisteredUserCommand command, CancellationToken cancellationToken)
    {
        var nonRegisteredUser = await _context.NonRegisteredUsers.FindAsync(command.NonRegisteredUserId);

        if (nonRegisteredUser == null)
        {
            return null;
        }

        var gymStaffAssignment = await _userProfileService.GetUserGymStaffAssigmentAsync(_user.Id!, cancellationToken);

        var userGymMembership = nonRegisteredUser.UserGymMemberships.FirstOrDefault(ugm => ugm.GymId == gymStaffAssignment!.GymId);

        if (userGymMembership != null)
        {
            return _mapper.Map<NonRegisteredUserDto>(nonRegisteredUser);
        }

        nonRegisteredUser.UserGymMemberships.Add(new UserGymMembership
        {
            Id = Guid.NewGuid().ToString(),
            UserId = nonRegisteredUser.Id,
            GymId = gymStaffAssignment!.GymId,
            GymMembershipStatus = GymMembershipStatus.Member,
        });

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<NonRegisteredUserDto>(nonRegisteredUser);
    }
}
