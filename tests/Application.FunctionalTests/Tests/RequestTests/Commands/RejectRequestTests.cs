using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Requests.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

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

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestIsNotFound()
    {
        await RunAsAppAdminAsync();

        var command = new RejectRequestCommand("invalidRequestId");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldContain($"{nameof(Request)} not found");
    }

    [Test]
    public async Task ShouldRejectRequest()
    {
        var obj = await TestEntityBuilder.BuildGymCreationRequest();

        await RunAsAppAdminAsync();

        var command = new RejectRequestCommand(obj.request.Id);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var request = await FindAsync<Request>(obj.request.Id);
        request.ShouldNotBeNull();
        request.Status.ShouldBe(RequestStatus.Rejected);
    }
}
