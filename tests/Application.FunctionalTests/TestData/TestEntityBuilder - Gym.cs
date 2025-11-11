using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
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
        GymMembershipPass unlimitedUsePass,
        GymMembershipPass expiredPass)> BuildGymAsync()
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

        var gymMember = await ApplicationUserBuilder
            .WithPassword("Password123_")
            .BuildAsync();

        var gymMemberUserProfile = await UserProfileBuilder
            .WithApplicationUserId(gymMember.Id)
            .BuildAsync();

        var gymMembership = await GymMembershipBuilder
            .WithApplicationUserId(gymMember.Id)
            .WithGym(obj.gym)
            .WithStatus(GymMembershipStatus.Active)
            .BuildAsync();

        var singleUsePass = await GymMembershipPassBuilder
            .WithGymMembership(gymMembership)
            .BuildAsync();

        var noUsePass = await GymMembershipPassBuilder
            .WithGymMembership(gymMembership)
            .AsMultiUseType(2, 0)
            .BuildAsync();

        var nowPlus10Days = DateTimeOffset.Now.AddDays(10);

        var unlimitedUsePass = await GymMembershipPassBuilder
            .WithGymMembership(gymMembership)
            .AsUnlimitedUseType(new DateOnly(nowPlus10Days.Year, nowPlus10Days.Month, nowPlus10Days.Day))
            .BuildAsync();

        var nowMinus10Days = DateTimeOffset.Now.AddDays(-10);

        var expiredPass = await GymMembershipPassBuilder
            .WithGymMembership(gymMembership)
            .AsUnlimitedUseType(new DateOnly(nowMinus10Days.Year, nowMinus10Days.Month, nowMinus10Days.Day))
            .BuildAsync();

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
            unlimitedUsePass,
            expiredPass);
    }

    public static CreateGymDto BuildCreateGymDto()
    {
        return new CreateGymDto
        {
            GymName = $"CreateGymDto GymName - {Guid.NewGuid()}",
            GymAddress = "Address",
            GymStatus = GymStatus.Active,
            GymTier = GymTier.Local,
            EscalationEmail = "escalation@email"
        };
    }
}
