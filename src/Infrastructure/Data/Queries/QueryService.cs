using FitPass.Application.Common.Interfaces;
using FitPass.Application.GymEmployments.DTOs;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Application.UserProfiles.DTOs;
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
                    Email = user.Email!
                }
            }
        ).ToListAsync();
    }

    public async Task<GymEmploymentDto?> GetGymEmploymentWithUserProfileAndEmailByApplicationUserId(string applicationUserId)
    {
        return await (
            from ge in _context.GymEmployments
            join user in _context.Users on ge.UserId equals user.Id
            join up in _context.UserProfiles on user.Id equals up.UserId
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
                    Email = user.Email!
                }
            }
        ).FirstOrDefaultAsync();
    }

    public async Task<UserProfileWithEmailDto?> GetUserProfileWithEmailByApplicationUserId(string applicationUserId)
    {
        return await (
            from up in _context.UserProfiles
            join user in _context.Users on up.UserId equals user.Id
            where up.UserId == applicationUserId
            select new UserProfileWithEmailDto
            {
                ApplicationUserId = user.Id,
                FirstName = up.FirstName,
                LastName = up.LastName,
                Email = user.Email!
            }
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
                    ApplicationUserId = user.Id,
                    GymId = gm.GymId!,
                    Status = gm.Status,
                    UserProfile = new UserProfileWithEmailDto
                    {
                        ApplicationUserId = user.Id,
                        FirstName = up.FirstName,
                        LastName = up.LastName,
                        Email = user.Email!
                    },
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
                ApplicationUserId = user.Id,
                GymId = gm.GymId!,
                Status = gm.Status,
                UserProfile = new UserProfileWithEmailDto
                {
                    ApplicationUserId = user.Id,
                    FirstName = up.FirstName,
                    LastName = up.LastName,
                    Email = user.Email!
                },
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
}
