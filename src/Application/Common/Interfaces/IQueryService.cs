using FitPass.Application.GymEmployments.DTOs;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Application.UserProfiles.DTOs;
using FitPass.Domain.Enums;

namespace FitPass.Application.Common.Interfaces;

public interface IQueryService
{
    Task<GymEmploymentDto?> GetGymEmploymentWithUserProfileAndEmailByUserId(string applicationUserId);
    Task<List<GymEmploymentDto>> GetGymEmploymentsWithUserProfileAndEmailByGymId(string gymId);
    Task<UserProfileWithEmailDto?> GetUserProfileWithEmailByApplicationUserId(string applicationUserId);
    Task<List<GymMembershipWithUserProfileAndEmailDto>> GetGymMembershipsWithUserProfilesAndEmailByGymIdAndMembershipStatus(string gymId, GymMembershipStatus? status);
    Task<GymMembershipWithUserProfileAndEmailDto?> GetGymMembershipWithUserProfileAndEmailByGymIdAndMembershipStatus(string gymMembershipId);
}
