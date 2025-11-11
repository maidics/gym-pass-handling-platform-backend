using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Requests.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.RequestTests.Commands;

using static Testing;

public class RejectRequestTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<RejectRequestCommand>(Roles.AppAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsAppAdminAsync();

        var command = new RejectRequestCommand(string.Empty);

        await ShouldThrowIfParametersAreInvalid(command);
    }

    [Test]
    public async Task ShouldThrowIfNotExists()
    {
        await RunAsAppAdminAsync();

        var command = new RejectRequestCommand("invalidRequestId");

        await ShouldThrowIfNotFound(command);
    }

    [Test]
    public async Task ShouldRejectRequest()
    {
        var obj = await TestEntityBuilder.BuildGymCreationRequest();

        await RunAsAppAdminAsync();

        var command = new RejectRequestCommand(obj.request.Id);

        await Should.NotThrowAsync(SendAsync(command));

        var request = await FindAsync<Request>(obj.request.Id);
        request.ShouldNotBeNull();
        request.Status.ShouldBe(Domain.Enums.RequestStatus.Rejected);
    }
}
