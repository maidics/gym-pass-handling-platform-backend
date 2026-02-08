using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassProducts.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
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
    public async Task ShouldThrowIfParametersAreInvalid()
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

        var command = new UpdateGymPassProductActiveStatusCommand("id", false);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldThrowIfProductHasNoPaymentIdentity()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymPassProductActiveStatusCommand(obj.gymPassProduct.Id, false);

        await Should.ThrowAsync<ArgumentNullException>(SendAsync(command));
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task ShouldUpdateGymPassProductActiveStatus(bool isActive, bool newStatus)
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();
        var product = await TestEntityBuilder.BuildGymPassProductWithPaymentProfile(
            obj.gymAdmin,
            new Money(10, CurrencyCode.USD),
            isActive: isActive
        );

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymPassProductActiveStatusCommand(product.Id, newStatus);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var updatedProduct = await FindAsync<GymPassProduct>(product.Id);
        updatedProduct.ShouldNotBeNull();
        updatedProduct.IsActive.ShouldBe(newStatus);
    }
}
