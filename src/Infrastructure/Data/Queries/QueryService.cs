using FitPass.Application.Common.Interfaces;
using FitPass.Application.GymEmployments.DTOs;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Application.UserProfiles.DTOs;
using FitPass.Application.Users.DTOs;
using FitPass.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitPass.Infrastructure.Data.Queries;

public class QueryService : IQueryService
{
    private readonly ApplicationDbContext _context;
    public QueryService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GymEmploymentDto>> GetGymEmploymentsWithUserProfileAndEmailByGymId(string gymId)
    {
        return await (
            from ge in _context.GymEmployments
            join user in _context.Users on ge.UserId equals user.Id
            join up in _context.UserProfiles on user.Id equals up.UserId
            where ge.GymId == gymId
            select new GymEmploymentDto
            {
                Id = ge.Id,
                UserId = user.Id,
                GymId = gymId,
                SupervisorEmail = ge.SupervisorEmail,
                Role = ge.Role,
                EmploymentStart = ge.EmploymentStart,
                EmploymentEnd = ge.EmploymentEnd,
                UserProfile = new UserProfileWithEmailDto(
                    user.Id,
                    up.FirstName,
                    up.LastName,
                    user.Email!,
                    up.PreferredLanguage
                    )
            }).ToListAsync();
    }

    public async Task<GymEmploymentDto?> GetGymEmploymentWithUserProfileAndEmailByUserId(string applicationUserId)
    {
        return await (
            from ge in _context.GymEmployments
            join user in _context.Users on ge.UserId equals user.Id
            join up in _context.UserProfiles on user.Id equals up.UserId
            where user.Id == applicationUserId
            select new GymEmploymentDto
            {
                Id = ge.Id,
                UserId = user.Id,
                GymId = ge.GymId,
                SupervisorEmail = ge.SupervisorEmail,
                Role = ge.Role,
                EmploymentStart = ge.EmploymentStart,
                EmploymentEnd = ge.EmploymentEnd,
                UserProfile = new UserProfileWithEmailDto(
                    user.Id,
                    up.FirstName,
                    up.LastName,
                    user.Email!,
                    up.PreferredLanguage)
            }
        ).FirstOrDefaultAsync();
    }

    public async Task<UserProfileWithEmailDto?> GetUserProfileWithEmailByApplicationUserId(string applicationUserId)
    {
        return await (
            from up in _context.UserProfiles
            join user in _context.Users on up.UserId equals user.Id
            where up.UserId == applicationUserId
            select new UserProfileWithEmailDto(
                user.Id,
                up.FirstName,
                up.LastName,
                user.Email!,
                up.PreferredLanguage)
        ).FirstOrDefaultAsync();
    }

    public async Task<List<GymMembershipWithUserProfileAndEmailDto>> GetGymMembershipsWithUserProfilesAndEmailByGymIdAndMembershipStatus(string gymId, GymMembershipStatus? status)
    {
        var query = (
                from gm in _context.GymMemberships
                join user in _context.Users on gm.UserId equals user.Id
                join up in _context.UserProfiles on user.Id equals up.UserId
                where gm.GymId == gymId
                select new GymMembershipWithUserProfileAndEmailDto
                {
                    Id = gm.Id,
                    UserId = user.Id,
                    GymId = gm.GymId!,
                    Status = gm.Status,
                    UserProfile = new UserProfileWithEmailDto(
                        user.Id,
                        up.FirstName,
                        up.LastName,
                        user.Email!,
                        up.PreferredLanguage),
                    Passes = gm.Passes.Select(p => new GymMembershipPassDto
                    {
                        Id = p.Id,
                        GymMembershipId = p.GymMembershipId,
                        Type = p.Type,
                        TotalUses = p.TotalUses,
                        RemainingUses = p.RemainingUses,
                        ExpirationDate = p.ExpirationDate,
                    }).ToList()
                }
            );

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
        }

        return await query.ToListAsync();
    }

    public async Task<GymEmploymentDto?> GetGymEmploymentWithUserProfileAndEmailByIdAsync(string gymEmploymentId, CancellationToken cancellationToken = default)
    {
        return await (
            from ge in _context.GymEmployments
            join user in _context.Users on ge.UserId equals user.Id
            join up in _context.UserProfiles on user.Id equals up.UserId
            where ge.Id == gymEmploymentId
            select new GymEmploymentDto
            {
                Id = ge.Id,
                UserId = user.Id,
                GymId = ge.GymId,
                SupervisorEmail = ge.SupervisorEmail,
                Role = ge.Role,
                EmploymentStart = ge.EmploymentStart,
                EmploymentEnd = ge.EmploymentEnd,
                UserProfile = new UserProfileWithEmailDto(
                    user.Id,
                    up.FirstName,
                    up.LastName,
                    user.Email!,
                    up.PreferredLanguage)
            }
        ).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GymMembershipWithUserProfileAndEmailDto?> GetGymMembershipWithUserProfileAndEmailByGymIdAndMembershipStatus(string gymMembershipId)
    {
        return await (
            from gm in _context.GymMemberships
            join user in _context.Users on gm.UserId equals user.Id
            join up in _context.UserProfiles on user.Id equals up.UserId
            where gm.Id == gymMembershipId
            select new GymMembershipWithUserProfileAndEmailDto
            {
                Id = gm.Id,
                UserId = user.Id,
                GymId = gm.GymId!,
                Status = gm.Status,
                UserProfile = new UserProfileWithEmailDto(
                    user.Id,
                    up.FirstName,
                    up.LastName,
                    user.Email!,
                    up.PreferredLanguage),
                Passes = gm.Passes.Select(p => new GymMembershipPassDto
                {
                    Id = p.Id,
                    GymMembershipId = p.GymMembershipId,
                    Type = p.Type,
                    TotalUses = p.TotalUses,
                    RemainingUses = p.RemainingUses,
                    ExpirationDate = p.ExpirationDate,
                }).ToList()
            }
        ).FirstOrDefaultAsync();
    }

    public async Task<string[]> GetGymEmployeeEmailsByGymIdAsync(string gymId)
    {
        return await (
            from ge in _context.GymEmployments
            join users in _context.Users on ge.UserId equals users.Id
            where ge.GymId == gymId
            select users.Email).ToArrayAsync();
    }

    public async Task<UserDto?> GetUserAsync(string userId)
    {
        return await (
            from user in _context.Users
            join profile in _context.UserProfiles on user.Id equals profile.UserId
            join employment in _context.GymEmployments on user.Id equals employment.UserId into empGroup
            from emp in empGroup.DefaultIfEmpty() //left join workaround
            where user.Id == userId
            select new UserDto(
                Id: user.Id,
                FirstName: profile.FirstName,
                LastName: profile.LastName,
                Email: user.Email,
                PreferredLanguage: profile.PreferredLanguage,
                CreatedOn: profile.CreatedOn,
                Roles: (from ur in _context.UserRoles
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where ur.UserId == user.Id
                    select r.Name).ToArray(),
                IsEmailConfirmed: user.EmailConfirmed,
                GymId: emp == null ? null : emp.GymId, //no null propagator in expression tree...
                GymEmploymentId: emp == null ? null : emp.Id)
            ).FirstOrDefaultAsync();
    }
}
