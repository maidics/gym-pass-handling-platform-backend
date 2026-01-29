using FitPass.Application.GymEmployments.DTOs;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Application.UserProfiles.DTOs;
using FitPass.Application.Users.DTOs;
using FitPass.Domain.Enums;

namespace FitPass.Application.Common.Interfaces;

public interface IQueryService
{
    Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken);
    
    Task<GymEmploymentDto?> GetGymEmploymentWithUserProfileAndEmailByUserId(string applicationUserId, CancellationToken cancellationToken);
    
    Task<List<GymEmploymentDto>> GetGymEmploymentsWithUserProfileAndEmailByGymId(string gymId, CancellationToken cancellationToken);
    
    Task<UserProfileWithEmailDto?> GetUserProfileWithEmailByApplicationUserId(string applicationUserId, CancellationToken cancellationToken);
    
    Task<List<GymMembershipWithUserProfileAndEmailDto>> GetGymMembershipsWithUserProfilesAndEmailByGymIdAndMembershipStatus(
        string gymId, GymMembershipStatus? status, CancellationToken cancellationToken);
    
    Task<GymEmploymentDto?> GetGymEmploymentWithUserProfileAndEmailByIdAsync(string gymEmploymentId, CancellationToken cancellationToken);
    
    Task<GymMembershipWithUserProfileAndEmailDto?> GetGymMembershipWithUserProfileAndEmailByGymIdAndMembershipStatus(
        string gymMembershipId, CancellationToken cancellationToken);
    
    Task<string[]> GetGymEmployeeEmailsByGymIdAsync(string gymId, CancellationToken cancellationToken);
}
