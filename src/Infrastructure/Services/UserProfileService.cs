using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitPass.Infrastructure.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserProfileService(IApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<UserGymMembership>?> GetUserGymMembershipsAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null || user.UserGymMemberships == null)
        {
            return null;
        }

        return [..user.UserGymMemberships];
    }

    public async Task<GymStaffAssigment?> GetUserGymStaffAssigment(string userId, CancellationToken cancellationToken)
    {
        return userId == null ? null : await _context.GymStaffAssigments.FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == userId);
    }
}