using FitPass.Application.GymEmployments.DTOs;
using FitPass.Application.GymMembershipPassUsages.DTOs;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Requests.DTOs;
using FitPass.Application.UserProfiles.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests;

public static class ApplicationEntityAssertions
{
    public static void AssertTo(this GymEmploymentDto? gymEmploymentDto, GymEmployment gymEmployment, UserProfile userProfile, ApplicationUser user)
    {
        gymEmploymentDto.ShouldNotBeNull();
        gymEmploymentDto.Role.ShouldBe(gymEmployment.Role);
        gymEmploymentDto.GymId.ShouldBe(gymEmployment.GymId);
        gymEmploymentDto.ApplicationUserId.ShouldBe(user.Id);
        gymEmploymentDto.EscalationEmail.ShouldBe(gymEmployment.EscalationEmail);
        gymEmploymentDto.UserProfile.Email.ShouldBe(user.Email!);
        gymEmploymentDto.UserProfile.FirstName.ShouldBe(userProfile.FirstName);
        gymEmploymentDto.UserProfile.LastName.ShouldBe(userProfile.LastName);
        gymEmploymentDto.UserProfile.ApplicationUserId.ShouldBe(user.Id);
    }

    public static void AssertToGym(this GymDto? gymDto, Gym gym)
    {
        gymDto.ShouldNotBeNull();
        gymDto.Id.ShouldBe(gym.Id);
        gymDto.Name.ShouldBe(gym.Name);
        gymDto.Address.ShouldBe(gym.Address);
        gymDto.Status.ShouldBe(gym.Status);
        gymDto.Tier.ShouldBe(gym.Tier);
        gymDto.OwnerName.ShouldBe(gym.OwnerName);
    }

    public static void AssertTo(this GymPassUsage? gymPassUsage, string userId, string gymId, GymMembershipPass pass, PassUseResult result, string? lockerNumber)
    {
        gymPassUsage.ShouldNotBeNull();
        gymPassUsage.ApplicationUserId.ShouldBe(userId);
        gymPassUsage.GymId.ShouldBe(gymId);
        gymPassUsage.PassType.ShouldBe(pass.Type);
        gymPassUsage.TotalPassUses.ShouldBe(pass.TotalUses);
        gymPassUsage.PassExpirationDate.ShouldBe(pass.ExpirationDate);
        gymPassUsage.PassUseResult.ShouldBe(result);
        gymPassUsage.LockerNumber.ShouldBe(lockerNumber);
        gymPassUsage.GymSessionEndedAt.ShouldBeNull();
        gymPassUsage.PassId.ShouldBe(pass.Id);

        if (result == PassUseResult.Success && pass.Type != PassType.Unlimited)
        {
            gymPassUsage.RemainingPassUses.ShouldBe(pass.RemainingUses - 1);
        } else
        {
            gymPassUsage.RemainingPassUses.ShouldBe(pass.RemainingUses);
        }
    }

    public static void AssertTo(this GymPassUsageDto? dto, GymPassUsage gymPassUsage)
    {
        dto.ShouldNotBeNull();
        dto.ApplicationUserId.ShouldBe(gymPassUsage.ApplicationUserId);
        dto.GymId.ShouldBe(gymPassUsage.GymId);
        dto.PassType.ShouldBe(gymPassUsage.PassType);
        dto.TotalPassUses.ShouldBe(gymPassUsage.TotalPassUses);
        dto.RemainingPassUses.ShouldBe(gymPassUsage.RemainingPassUses);
        dto.PassExpirationDate.ShouldBe(gymPassUsage.PassExpirationDate);
        dto.PassUseResult.ShouldBe(gymPassUsage.PassUseResult);
        dto.LockerNumber.ShouldBe(gymPassUsage.LockerNumber);
        dto.GymSessionEndedAt.ShouldBe(gymPassUsage.GymSessionEndedAt);
    }

    public static void AssertToCreateGymDto(this GymDto? gymDto, CreateGymDto createGymDto)
    {
        gymDto.ShouldNotBeNull();
        gymDto.Name.ShouldBe(createGymDto.GymName);
        gymDto.Address.ShouldBe(createGymDto.GymAddress);
        gymDto.Status.ShouldBe(createGymDto.GymStatus);
        gymDto.Tier.ShouldBe(createGymDto.GymTier);
        gymDto.OwnerName.ShouldBe(createGymDto.GymOwnerName);
    }

    public static void AssertToDto(this Gym? gym, GymDto dto)
    {
        gym.ShouldNotBeNull();
        gym.Id.ShouldBe(dto.Id);
        gym.Name.ShouldBe(dto.Name);
        gym.Address.ShouldBe(dto.Address);
        gym.Status.ShouldBe(dto.Status);
        gym.Tier.ShouldBe(dto.Tier);
        gym.CreatedOn.ShouldBe(dto.CreatedOn);
        gym.OwnerName.ShouldBe(dto.OwnerName);
    }

    public static void AssertTo(this UserProfileWithEmailDto? dto, UserProfile userProfile, string email)
    {
        dto.ShouldNotBeNull();
        dto.ApplicationUserId.ShouldBe(userProfile.ApplicationUserId);
        dto.FirstName.ShouldBe(userProfile.FirstName);
        dto.LastName.ShouldBe(userProfile.LastName);
        dto.Email.ShouldBe(email);
    }

    public static void AssertTo(this GymMembershipDto? dto, GymMembership gymMembership, UserProfile userProfile, string email)
    {
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(gymMembership.Id);
        dto.ApplicationUserId.ShouldBe(gymMembership.ApplicationUserId);
        dto.GymId.ShouldBe(gymMembership.GymId);
        dto.Status.ShouldBe(gymMembership.Status);
        dto.CreatedOn.ShouldBe(gymMembership.CreatedOn);
        dto.CreatedBy.ShouldBe(gymMembership.CreatedBy);
        dto.Passes.ShouldBeEquivalentTo(gymMembership.Passes);
    }
}
