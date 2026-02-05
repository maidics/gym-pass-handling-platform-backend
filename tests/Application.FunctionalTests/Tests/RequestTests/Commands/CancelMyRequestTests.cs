using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.Requests.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.RequestTests.Commands;

using static Testing;

public class CancelMyRequestTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<CancelMyRequestCommand>(
            Roles.User,
            Roles.PendingGymEmployee,
            Roles.GymStaff,
            Roles.GymAdministrator
        );
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestIsNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new CancelMyRequestCommand("id");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfRequestIsNotInSubmittedStatus()
    {
        var obj = await RunAsDefaultUserAsync();

        var request = new Request()
        {
            CreatedBy = obj.user.Id, //TODO: check if works
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.High,
            Type = RequestType.Other,
            Status = RequestStatus.Approved,
            Payload = null,
        };

        await AddAsync(request);

        var command = new CancelMyRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldCancelRequest()
    {
        var obj = await RunAsDefaultUserAsync();

        var request = new Request()
        {
            CreatedBy = obj.user.Id, //TODO: check if works
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.High,
            Type = RequestType.Other,
            Status = RequestStatus.Submitted,
            Payload = null,
        };

        await AddAsync(request);

        var command = new CancelMyRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();
    }
}
