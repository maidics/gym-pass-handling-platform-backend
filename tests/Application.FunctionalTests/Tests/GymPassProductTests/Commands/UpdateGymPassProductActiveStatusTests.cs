using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassProducts.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.FunctionalTests.Tests.GymPassProductTests.Commands;

using static Testing;

public class UpdateGymPassProductActiveStatusTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateGymPassProductActiveStatusCommand>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);
        
        var command = new UpdateGymPassProductActiveStatusCommand(string.Empty, true);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymPassProductIsNotFound()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymPassProductActiveStatusCommand("gymPassProductId", false);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
    }
    
    [TestCase(true, true)]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task ShouldUpdateGymPassProductActiveStatus(bool isActive, bool newStatus)
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();
        var product = await TestEntityBuilder.BuildGymPassProduct(obj.gymAdmin, Money.Zero("usd"), isActive: isActive);

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymPassProductActiveStatusCommand(product.Id, newStatus);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        product = await FindAsync<GymPassProduct>(product.Id);
        product.ShouldNotBeNull();
        product.IsActive.ShouldBe(newStatus);
    }
}
