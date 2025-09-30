using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitPass.Infrastructure.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IApplicationDbContext _context;

    public UserProfileService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserGymMembership?> GetUserGymMembershipAsync(string userId, string gymId, CancellationToken cancellationToken)
    {
        var ugm = await _context
            .UserGymMemberships
            .Include(ugm => ugm.OwnedPasses)
            .FirstOrDefaultAsync(ugm => ugm.UserId == userId && ugm.GymId == gymId);

        return ugm;
    }

    public async Task<GymStaffAssigment?> GetUserGymStaffAssigmentAsync(string userId, CancellationToken cancellationToken)
    {
        return userId == null ? null : await _context.GymStaffAssigments.FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == userId);
    }
}