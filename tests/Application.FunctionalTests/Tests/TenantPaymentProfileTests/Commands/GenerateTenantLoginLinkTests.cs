using System;
using System.Collections.Generic;
using System.Text;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.TenantPaymentProfiles.Commands;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.TenantPaymentProfileTests.Commands;

/*
using static Testing;

public class GenerateTenantLoginLinkTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GenerateTenantLoginLinkCommand>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfPaymentProfileNotFound()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new GenerateTenantLoginLinkCommand();

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnLoginLink()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new GenerateTenantLoginLinkCommand();

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }
}
*/
