using FitPass.Application.GymStaffAssignments.DTOs;
using FitPass.Application.UserProfiles.DTOs;

namespace FitPass.Application.Common.Interfaces;

public interface IQueryService
{
    Task<GymEmploymentDto?> GetGymEmploymentWithUserProfileAndEmailByApplicationUserId(string applicationUserId);
    Task<List<GymEmploymentDto>> GetGymEmploymentsWithUserProfileAndEmailByGymId(string gymId);
    Task<UserProfileWithEmailDto?> GetUserProfileWithEmailByApplicationUserId(string applicationUserId);
}
