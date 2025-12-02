using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
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
        UserProfile gymStaffUserProfile,
        ApplicationUser gymMember,
        UserProfile gymMemberUserProfile,
        GymMembership gymMembership,
        GymMembershipPass singleUsePass,
        GymMembershipPass noUsePass,
        GymPassUsage passUsage,
        GymMembershipPass unlimitedUsePass)> BuildGymAsync()
    {
        var obj = await BuildGymEmployeeAsync(Roles.GymAdministrator);

        var gymStaff = await CreateUserAsync(Roles.GymStaff);

        var gymStaffGymEmployment = new GymEmployment
        {
            UserId = gymStaff.Id,
            GymId = obj.gym.Id,
            Role = Roles.GymStaff,
            EmploymentStart = GetUtcNow()
        };

        await AddAsync(gymStaffGymEmployment);

        var gymStaffUserProfile = new UserProfile
        {
            UserId = gymStaff.Id,
            FirstName = "Gym",
            LastName = "Staff",
        };

        await AddAsync(gymStaffUserProfile);

        var gymMember = await CreateUserAsync();

        var gymMemberUserProfile = new UserProfile
        {
            UserId = gymMember.Id,
            FirstName = "Gym",
            LastName = "Member",
        };

        await AddAsync(gymMemberUserProfile);

        var gymMembership = new GymMembership
        {
            GymId = obj.gym.Id,
            UserId = gymMember.Id,
            Status = GymMembershipStatus.Active
        };

        await AddAsync(gymMembership);

        var singleUsePass = GymPassProduct
            .SingleUse(obj.gym.Id, "Test Product", "Test Description", true, Money.Eur(10))
            .ToGymMembershipPass(gymMembership.Id, gymMember.Id, GetUtcNow());

        await AddAsync(singleUsePass);

        var noUsePass = GymPassProduct
            .SingleUse(obj.gym.Id, "Test Product", "Test Description", true, Money.Eur(10))
            .ToGymMembershipPass(gymMembership.Id, gymMember.Id, GetUtcNow());

        var passUsage = noUsePass.Use("Test Locker", GetUtcNow());
        passUsage.EndGymSession(GetUtcNow());

        await AddAsync(noUsePass);
        await AddAsync(passUsage);

        var unlimitedUsePass = GymPassProduct
            .UnlimitedUse(obj.gym.Id, "Test Name", "Test Description", 10, true, Money.Eur(10))
            .ToGymMembershipPass(gymMembership.Id, gymMember.Id, GetUtcNow());

        await AddAsync(unlimitedUsePass);

        return (
            obj.gym,
            obj.user,
            obj.gymEmployment,
            obj.userProfile,
            gymStaff,
            gymStaffGymEmployment,
            gymStaffUserProfile,
            gymMember,
            gymMemberUserProfile,
            gymMembership,
            singleUsePass,
            noUsePass,
            passUsage,
            unlimitedUsePass);
    }
}
