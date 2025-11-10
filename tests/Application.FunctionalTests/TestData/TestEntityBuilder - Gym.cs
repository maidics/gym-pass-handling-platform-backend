using System;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.TestData;

using static Testing;

public partial class TestEntityBuilder
{
    public static async Task<(
        Gym gym,
        ApplicationUser gymAdmin,
        GymEmployment gymAdminGymEmployment,
        UserProfile gymAdminUserProfile,
        ApplicationUser gymStaff,
        GymEmployment gymStaffGymEmployment,
        UserProfile gymStaffUserProfile)> BuildGymAsync()
    {
        var obj = await BuildGymEmployeeAsync(Roles.GymAdministrator);

        var gymStaff = await ApplicationUserBuilder
            .WithRole(Roles.GymStaff)
            .WithPassword("Password123_")
            .BuildAsync();

        var gymStaffGymEmployment = await GymEmploymentBuilder
            .WithGym(obj.gym)
            .WithApplicationUserId(gymStaff.Id)
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        var gymStaffUserProfile = await UserProfileBuilder
            .WithApplicationUserId(gymStaff.Id)
            .BuildAsync();

        return (obj.gym, obj.user, obj.gymEmployment, obj.userProfile, gymStaff, gymStaffGymEmployment, gymStaffUserProfile);
    }
}
