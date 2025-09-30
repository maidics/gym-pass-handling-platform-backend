using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IUserProfileService
{
    Task<GymStaffAssigment?> GetUserGymStaffAssigmentAsync(string userId, CancellationToken cancellationToken);
    Task<UserGymMembership?> GetUserGymMembershipAsync(string userId, string gymId, CancellationToken cancellationToken);
}
