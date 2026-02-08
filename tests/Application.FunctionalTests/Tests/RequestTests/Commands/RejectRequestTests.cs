using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;

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
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsAppAdminAsync();

        var command = new RejectRequestCommand(string.Empty, null);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestIsNotFound()
    {
        await RunAsAppAdminAsync();

        var command = new RejectRequestCommand("id", null);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldRejectRequest()
    {
        var obj = await TestEntityBuilder.BuildGymCreationRequest();

        await RunAsAppAdminAsync();

        var command = new RejectRequestCommand(obj.request.Id, "Rationale");

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var request = await FindAsync<Request>(obj.request.Id);
        request.ShouldNotBeNull();
        request.Status.ShouldBe(RequestStatus.Rejected);
        request.HandlerRationale.ShouldBe(command.Rationale);
    }
}
