using FitPass.Application.Common.Interfaces;
using FitPass.Application.GymEmployments.DTOs;
using FitPass.Application.UserProfiles.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FitPass.Infrastructure.Data.Queries;

public class QueryService : IQueryService
{
    private readonly ApplicationDbContext _context;
    public QueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GymEmploymentDto>> GetGymEmploymentsWithUserProfileAndEmailByGymId(string gymId)
    {
        return await (
            from ge in _context.GymEmployments
            join user in _context.Users on ge.ApplicationUserId equals user.Id
            join up in _context.UserProfiles on user.Id equals up.ApplicationUserId
            where ge.GymId == gymId
            select new GymEmploymentDto
            {
                ApplicationUserId = user.Id,
                GymId = gymId,
                EscalationEmail = ge.EscalationEmail,
                Role = ge.Role,
                EmploymentStart = ge.EmploymentStart,
                EmploymentEnd = ge.EmploymentEnd,
                UserProfile = new UserProfileWithEmailDto
                {
                    ApplicationUserId = user.Id,
                    FirstName = up.FirstName,
                    LastName = up.LastName,
                    Email = user.Email
                }
            }
        ).ToListAsync();
    }

    public async Task<GymEmploymentDto?> GetGymEmploymentWithUserProfileAndEmailByApplicationUserId(string applicationUserId)
    {
        return await (
            from ge in _context.GymEmployments
            join user in _context.Users on ge.ApplicationUserId equals user.Id
            join up in _context.UserProfiles on user.Id equals up.ApplicationUserId
            where user.Id == applicationUserId
            select new GymEmploymentDto
            {
                ApplicationUserId = user.Id,
                GymId = ge.GymId,
                EscalationEmail = ge.EscalationEmail,
                Role = ge.Role,
                EmploymentStart = ge.EmploymentStart,
                EmploymentEnd = ge.EmploymentEnd,
                UserProfile = new UserProfileWithEmailDto
                {
                    ApplicationUserId = user.Id,
                    FirstName = up.FirstName,
                    LastName = up.LastName,
                    Email = user.Email
                }
            }
        ).FirstOrDefaultAsync();
    }

    public async Task<UserProfileWithEmailDto?> GetUserProfileWithEmailByApplicationUserId(string applicationUserId)
    {
        return await (
            from up in _context.UserProfiles
            join user in _context.Users on up.ApplicationUserId equals user.Id
            where up.ApplicationUserId == applicationUserId
            select new UserProfileWithEmailDto
            {
                ApplicationUserId = user.Id,
                FirstName = up.FirstName,
                LastName = up.LastName,
                Email = user.Email
            }
        ).FirstOrDefaultAsync();
    }
}
