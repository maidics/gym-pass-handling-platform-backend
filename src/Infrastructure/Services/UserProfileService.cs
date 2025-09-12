using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitPass.Infrastructure.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;

    public UserProfileService(IUser user, IApplicationDbContext context)
    {
        _user = user;
        _context = context;
    }

    public async Task<GymStaffAssigment?> GetUserGymStaffAssigment(CancellationToken cancellationToken)
    {
        return _user.Id == null ? null : await _context.GymStaffAssigments.FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id);
    }
}