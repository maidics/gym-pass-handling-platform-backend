using FitPass.Application.GymEmployments.DTOs;
using FitPass.Application.GymMembershipPassUsages.DTOs;
using FitPass.Application.Gyms.DTOs;
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

    public static void AssertTo(this GymDto? gymDto, Gym gym)
    {
        gymDto.ShouldNotBeNull();
        gymDto.Id.ShouldBe(gym.Id);
        gymDto.Name.ShouldBe(gym.Name);
        gymDto.Address.ShouldBe(gym.Address);
        gymDto.Status.ShouldBe(gym.Status);
        gymDto.Tier.ShouldBe(gym.Tier);
        gymDto.OwnerName.ShouldBe(gym.OwnerName);
        gymDto.GymPassProducts.ShouldBeEquivalentTo(gym.PassProducts);
    }

    public static void AssertTo(this GymPassUsage? gymPassUsage, string userId, string gymId, GymMembershipPass pass, PassUseResult result, string? lockerNumber)
    {
        gymPassUsage.ShouldNotBeNull();
        gymPassUsage.ApplicationUserId.ShouldBe(userId);
        gymPassUsage.GymId.ShouldBe(gymId);
        gymPassUsage.PassType.ShouldBe(pass.Type);
        gymPassUsage.TotalPassUses.ShouldBe(pass.TotalUses);
        gymPassUsage.RemainingPassUses.ShouldBe(pass.RemainingUses);
        gymPassUsage.PassExpirationDate.ShouldBe(pass.ExpirationDate);
        gymPassUsage.PassUseResult.ShouldBe(result);
        gymPassUsage.LockerNumber.ShouldBe(lockerNumber);
        gymPassUsage.GymSessionEndedAt.ShouldBeNull();
        gymPassUsage.PassId.ShouldBe(pass.Id);
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
}
