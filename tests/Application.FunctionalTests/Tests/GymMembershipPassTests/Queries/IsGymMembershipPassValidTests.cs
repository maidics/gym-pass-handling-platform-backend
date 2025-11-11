using System;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymMembershipPasses.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymMembershipPassTests.Queries;

using static Testing;

public class IsGymMembershipPassValidTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<IsGymMembershipPassValidQuery>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new IsGymMembershipPassValidQuery(string.Empty);

        await ShouldThrowIfParametersAreInvalid(command);
    }

    [Test]
    public async Task ShouldThrowIfNotExists()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new IsGymMembershipPassValidQuery("invalidPassId");

        await ShouldThrowIfNotFound(command);
    }

    [Test]
    public async Task ShouldReturnTrue()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymStaff);

        var command = new IsGymMembershipPassValidQuery(obj.singleUsePass.Id);

        var result = await SendAsync(command);
        result.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnFalse()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymStaff);

        var command = new IsGymMembershipPassValidQuery(obj.expiredPass.Id);

        var result = await SendAsync(command);
        result.ShouldBeFalse();
    }
}
