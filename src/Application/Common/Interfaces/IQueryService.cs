using FitPass.Application.GymEmployments.DTOs;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Application.UserProfiles.DTOs;
using FitPass.Application.Users.DTOs;
using FitPass.Domain.Enums;

namespace FitPass.Application.Common.Interfaces;

public interface IQueryService
{
    Task<UserDto?> GetUserAsync(string userId);
    Task<GymEmploymentDto?> GetGymEmploymentWithUserProfileAndEmailByUserId(string applicationUserId);
    Task<List<GymEmploymentDto>> GetGymEmploymentsWithUserProfileAndEmailByGymId(string gymId);
    Task<UserProfileWithEmailDto?> GetUserProfileWithEmailByApplicationUserId(string applicationUserId);
    Task<List<GymMembershipWithUserProfileAndEmailDto>> GetGymMembershipsWithUserProfilesAndEmailByGymIdAndMembershipStatus(string gymId, GymMembershipStatus? status);
    Task<GymEmploymentDto?> GetGymEmploymentWithUserProfileAndEmailByIdAsync(string gymEmploymentId, CancellationToken cancellationToken = default);
    Task<GymMembershipWithUserProfileAndEmailDto?> GetGymMembershipWithUserProfileAndEmailByGymIdAndMembershipStatus(string gymMembershipId);
    Task<string[]> GetGymEmployeeEmailsByGymIdAsync(string gymId);
}
