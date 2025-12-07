using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.TenantPaymentProfiles.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.TenantPaymentProfileTests.Commands;

using static Testing;

public class UpdateTenantPaymentAccountPayoutScheduleTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateTenantPaymentAccountPayoutScheduleCommand>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfGymHasNoPaymentAccount()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateTenantPaymentAccountPayoutScheduleCommand(
            Interval: TimeIntervals.Daily,
            DelayDays: 1);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldContain("Gym has no payment account created");
    }

    [Test]
    public async Task ShouldUpdatePayoutSchedule()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateTenantPaymentAccountPayoutScheduleCommand(
            Interval: TimeIntervals.Weekly,
            WeeklyAnchor: DayOfWeek.Wednesday);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
    }
}
