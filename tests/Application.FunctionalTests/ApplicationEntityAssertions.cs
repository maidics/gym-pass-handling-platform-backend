using FitPass.Application.GymEmployments.DTOs;
using FitPass.Domain.Entities;
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
}
