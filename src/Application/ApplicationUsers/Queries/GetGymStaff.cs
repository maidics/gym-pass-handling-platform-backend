using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace Fitpass.Application.ApplicationUsers.Queries;

[Authorize(Roles = $"{Roles.AppAdministrator},{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymStaffQuery(string GymId) : IRequest<(List<ApplicationUserDto>? gymStaffManagementUsers, string? errorMessage)>;

public class GetGymStaffQueryValidator : AbstractValidator<GetGymStaffQuery>
{
    public GetGymStaffQueryValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage("Gym id");
    }
}

public class GetGymStaffQueryHandler : IRequestHandler<GetGymStaffQuery, (List<ApplicationUserDto>? gymStaffManagementUsers, string? errorMessage)>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetGymStaffQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<(List<ApplicationUserDto>? gymStaffManagementUsers, string? errorMessage)> Handle(GetGymStaffQuery query, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms.AsNoTracking().FirstOrDefaultAsync(g => g.Id == query.GymId, cancellationToken);

        if (gym == null)
        {
            return (null, "Gym not found");
        }

        if (_user.Roles!.Contains(Roles.GymAdministrator) || _user.Roles!.Contains(Roles.GymStaff))
        {
            var currentGymManagementUser = await _context
                .ApplicationUsers
                .AsNoTracking()
                .Include(u => u.GymStaffAssigment)
                .FirstOrDefaultAsync(u => u.GymStaffAssigment!.GymId == query.GymId && u.Id == _user.Id, cancellationToken);

            if (currentGymManagementUser == null)
            {
                return (null, "You are not allowed to request this.");
            }
        }

        var gymStaffAssigments = await _context
            .GymStaffAssigments
            .Include(gsa => gsa.ApplicationUser)
            .Where(gsa => gsa.GymId == query.GymId)
            .ToListAsync(cancellationToken);

        List<ApplicationUserDto> gymManagementUsers = [];

        gymStaffAssigments.ForEach(gsa =>
        {
            gymManagementUsers.Add(new ApplicationUserDto
            {
                Id = gsa.ApplicationUser.Id,
                Email = gsa.ApplicationUser.Email!,
                FirstName = gsa.ApplicationUser.FirstName,
                LastName = gsa.ApplicationUser.LastName,
                UserGymMemberships = null,
                GymStaffAssigment = gsa
            });
        });

        return (gymManagementUsers, null);
    }
}