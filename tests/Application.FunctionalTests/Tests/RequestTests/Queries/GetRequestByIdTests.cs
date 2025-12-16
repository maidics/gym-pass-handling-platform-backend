using FitPass.Application.Common.Models;
using FitPass.Application.Requests.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.RequestTests.Queries;

using static Testing;

public class GetRequestByIdTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetRequestByIdQuery>(Roles.AppAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsAppAdminAsync();

        var query = new GetRequestByIdQuery(RequestId: "");

        await ShouldThrowIfParametersAreInvalidAsync(query);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestIsNotFound()
    {
        await RunAsAppAdminAsync();

        var query = new GetRequestByIdQuery("requestId");

        var result = await SendAsync(query);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnRequest()
    {
        await RunAsAppAdminAsync();

        var request = new Request
        {
            Title = "Test Request",
            Description = "Test Request",
            Status = RequestStatus.Submitted,
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.Other,
            Payload = null
        };

        await AddAsync(request);

        var query = new GetRequestByIdQuery(request.Id);

        var result = await SendAsync(query);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var requestDto = result.Value;
        requestDto.AssertTo(request);
    }
}
