using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.Requests.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.RequestTests.Queries;

using static Testing;

public class GetMyRequestByIdTests : BaseTestFixture
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
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        var query = new GetMyRequestByIdQuery(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(query);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestIsNotFound()
    {
        await RunAsDefaultUserAsync();

        var query = new GetMyRequestByIdQuery("id");

        var result = await SendAsync(query);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserIsNotTheCreatorOfRequest()
    {
        var request = new Request()
        {
            CreatedBy = "id", //TODO: test if this works
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.High,
            Status = RequestStatus.Rejected,
            Type = RequestType.Other,
            Payload = null,
        };

        await AddAsync(request);

        await RunAsDefaultUserAsync();

        var query = new GetMyRequestByIdQuery(request.Id);

        var result = await SendAsync(query);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnRequest()
    {
        var obj = await RunAsDefaultUserAsync();

        var request = new Request()
        {
            CreatedBy = obj.user.Id, //TODO: test if this works
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.High,
            Status = RequestStatus.Rejected,
            Type = RequestType.Other,
            Payload = null,
        };

        await AddAsync(request);

        var query = new GetMyRequestByIdQuery(request.Id);

        var result = await SendAsync(query);
        result.ShouldBeSuccessful();

        var dto = result.Value;
        dto.Id.ShouldBe(request.Id);
        dto.CreatedBy.ShouldBe(obj.user.Id);
    }
}
