using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
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
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsAppAdminAsync();

        var query = new GetRequestByIdQuery(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(query);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestIsNotFound()
    {
        await RunAsAppAdminAsync();

        var query = new GetRequestByIdQuery("id");

        var result = await SendAsync(query);
        result.ShouldBeFailed(ResultTypes.NotFound);
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
            Payload = null,
        };

        await AddAsync(request);

        var query = new GetRequestByIdQuery(request.Id);

        var result = await SendAsync(query);
        result.ShouldBeSuccessful();

        var dto = result.Value;

        dto.Id.ShouldBe(request.Id);
    }
}
