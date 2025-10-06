using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.NonRegisteredUsers.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.NonRegisteredUsers.Commands;
[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record AddUserGymMembershipToNonRegisteredUserCommand(
    string NonRegisteredUserId
) : IRequest<NonRegisteredUserDto>;

public class AddUserGymMembershipToNonRegisteredUserCommandValidator : AbstractValidator<AddUserGymMembershipToNonRegisteredUserCommand>
{
    public AddUserGymMembershipToNonRegisteredUserCommandValidator()
    {
        RuleFor(v => v.NonRegisteredUserId).NotEmptyWithMessage("User id");
    }
}


public class AddUserGymMembershipToNonRegisteredUserCommandHandler : IRequestHandler<AddUserGymMembershipToNonRegisteredUserCommand, NonRegisteredUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public AddUserGymMembershipToNonRegisteredUserCommandHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }
    public async Task<NonRegisteredUserDto> Handle(AddUserGymMembershipToNonRegisteredUserCommand command, CancellationToken cancellationToken)
    {
        var nonRegisteredUser = await _context.NonRegisteredUsers.FindAsync(command.NonRegisteredUserId);

        Guard.Against.NotFound(command.NonRegisteredUserId, nonRegisteredUser, "Id");

        var gymStaffAssignment = await _context.GymStaffAssigments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        var userGymMembership = nonRegisteredUser.UserGymMemberships.FirstOrDefault(ugm => ugm.GymId == gymStaffAssignment!.GymId);

        if (userGymMembership != null)
        {
            return _mapper.Map<NonRegisteredUserDto>(nonRegisteredUser);
        }

        nonRegisteredUser.UserGymMemberships.Add(new UserGymMembership
        {
            UserId = nonRegisteredUser.Id,
            GymId = gymStaffAssignment!.GymId!,
        });

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<NonRegisteredUserDto>(nonRegisteredUser);
    }
}
