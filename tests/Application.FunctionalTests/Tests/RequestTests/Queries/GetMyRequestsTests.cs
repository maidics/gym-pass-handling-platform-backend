
using FitPass.Application.Requests.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.RequestTests.Queries;

using static Testing;

public class GetMyRequestsTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetMyRequestByIdQuery>(
            Roles.User,
            Roles.PendingGymEmployee,
            Roles.GymStaff,
            Roles.GymAdministrator
        );
    }

    [Test]
    public async Task ShouldReturnRequests()
    {
        var obj = await RunAsDefaultUserAsync();

        var request1 = new Request()
        {
            CreatedBy = obj.user.Id, //TODO: test if this works
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.High,
            Status = RequestStatus.Rejected,
            Type = RequestType.Other,
            Payload = null,
        };

        var request2 = new Request()
        {
            CreatedBy = obj.user.Id, //TODO: test if this works
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.High,
            Status = RequestStatus.Rejected,
            Type = RequestType.Other,
            Payload = null,
        };

        var query = new GetMyRequestsQuery();

        var result = await SendAsync(query);
        result.Count.ShouldBe(2);
        result.Count(x => x.Id == request1.Id || x.Id == request2.Id).ShouldBe(2);
    }
}
