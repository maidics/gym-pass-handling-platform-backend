using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IUserProfileService
{
    Task<GymStaffAssigment?> GetUserGymStaffAssigment(CancellationToken cancellationToken);
}