using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IUserProfileService
{
    Task<GymStaffAssigment?> GetUserGymStaffAssigmentAsync(string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserGymMembership>?> GetUserGymMembershipsAsync(string userId, CancellationToken cancellationToken);
}
