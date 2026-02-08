
using FitPass.Application.Requests.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.RequestTests.Queries;

using static Testing;

public class GetRequestsTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetRequestsQuery>(Roles.AppAdministrator);
    }

    [Test]
    public async Task ShouldReturnRequests()
    {
        await RunAsAppAdminAsync();

        var request1 = new Request
        {
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.GymCreation,
            Status = RequestStatus.Approved,
            Payload = null,
        };

        await AddAsync(request1);

        var request2 = new Request
        {
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.Other,
            Status = RequestStatus.Submitted,
            Payload = null,
        };

        await AddAsync(request2);

        var query = new GetRequestsQuery();

        var result = await SendAsync(query);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Count(x => x.Id == request1.Id || x.Id == request2.Id).ShouldBe(2);
    }
}
