using FitPass.Application.FunctionalTests.Infrastructure.Testing;
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
    public async Task ShouldReturnByRequestType()
    {
        await RunAsAppAdminAsync();

        var requestNotOther = new Request
        {
            Title = "Request 1",
            Description = "Description 1",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.GymCreation,
            Status = RequestStatus.Submitted,
            Payload = null,
        };

        await AddAsync(requestNotOther);

        var requestOther = new Request
        {
            Title = "Request 1",
            Description = "Description 1",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.Other,
            Status = RequestStatus.Submitted,
            Payload = null,
        };

        await AddAsync(requestOther);

        var query = new GetRequestsQuery();

        var result = await SendAsync(query);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Type.ShouldBe(RequestType.Other);
    }

    [Test]
    public async Task ShouldReturnByRequestStatus()
    {
        await RunAsAppAdminAsync();

        var requestNotSubmitted = new Request
        {
            Title = "Request 1",
            Description = "Description 1",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.GymCreation,
            Status = RequestStatus.Approved,
            Payload = null,
        };

        await AddAsync(requestNotSubmitted);

        var requestSubmitted = new Request
        {
            Title = "Request 1",
            Description = "Description 1",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.Other,
            Status = RequestStatus.Submitted,
            Payload = null,
        };

        await AddAsync(requestSubmitted);

        var query = new GetRequestsQuery();

        var result = await SendAsync(query);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Status.ShouldBe(RequestStatus.Submitted);
    }

    [Test]
    public async Task ShouldReturnAllRequests()
    {
        await RunAsAppAdminAsync();

        var requestNotSubmitted = new Request
        {
            Title = "Request 1",
            Description = "Description 1",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.GymCreation,
            Status = RequestStatus.Approved,
            Payload = null,
        };

        await AddAsync(requestNotSubmitted);

        var requestSubmitted = new Request
        {
            Title = "Request 1",
            Description = "Description 1",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.Other,
            Status = RequestStatus.Submitted,
            Payload = null,
        };

        await AddAsync(requestSubmitted);

        var query = new GetRequestsQuery();

        var result = await SendAsync(query);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Test]
    public async Task ShouldReturnByRequestTypeAndStatus()
    {
        await RunAsAppAdminAsync();
        var request1 = new Request
        {
            Title = "Request 1",
            Description = "Description 1",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.GymCreation,
            Status = RequestStatus.Approved,
            Payload = null,
        };

        await AddAsync(request1);

        var request2 = new Request
        {
            Title = "Request 2",
            Description = "Description 2",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.GymCreation,
            Status = RequestStatus.Submitted,
            Payload = null,
        };

        await AddAsync(request2);

        var request3 = new Request
        {
            Title = "Request 3",
            Description = "Description 3",
            PriorityLevel = PriorityLevel.Medium,
            Type = RequestType.Other,
            Status = RequestStatus.Submitted,
            Payload = null,
        };

        await AddAsync(request3);

        var query = new GetRequestsQuery();
        var result = await SendAsync(query);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Type.ShouldBe(RequestType.GymCreation);
        result[0].Status.ShouldBe(RequestStatus.Submitted);
    }
}
