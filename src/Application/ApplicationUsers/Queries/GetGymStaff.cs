using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace Fitpass.Application.ApplicationUsers.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetGymStaffQuery(string GymId) : IRequest<List<ApplicationUserDto>>;

public class GetGymStaffQueryValidator : AbstractValidator<GetGymStaffQuery>
{
    public GetGymStaffQueryValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage("Gym id");
    }
}

public class GetGymStaffQueryHandler : IRequestHandler<GetGymStaffQuery, List<ApplicationUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetGymStaffQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<List<ApplicationUserDto>> Handle(GetGymStaffQuery query, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms.AsNoTracking().FirstOrDefaultAsync(g => g.Id == query.GymId, cancellationToken);

        Guard.Against.NotFound(query.GymId, gym, "GymId");

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

        return gymManagementUsers;
    }
}