using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
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
        ShouldRequireAuthorization<IsGymMembershipPassValidQuery>(
            Roles.GymAdministrator,
            Roles.GymStaff
        );
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new IsGymMembershipPassValidQuery(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfNotFound()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new IsGymMembershipPassValidQuery("invalidPassId");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnTrue()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymStaff);

        var command = new IsGymMembershipPassValidQuery(obj.singleUsePass.Id);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnFalse()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymStaff);

        var command = new IsGymMembershipPassValidQuery(obj.noUsePass.Id);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();
        result.Value.ShouldBeFalse();
    }
}
