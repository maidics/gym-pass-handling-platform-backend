using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.Requests.Commands.Fulfill;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.RequestTests.Commands.Fulfill;

using static Testing;

public class FulFillOtherTypeRequestTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<FulfillOtherTypeRequestCommand>(Roles.AppAdministrator);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestIsNotFound()
    {
        await RunAsAppAdminAsync();

        var command = new FulfillOtherTypeRequestCommand("id");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfRequestIsNotOfOtherType()
    {
        await RunAsAppAdminAsync();

        var request = new Request()
        {
            Title = "Title",
            Description = "Description",
            Type = RequestType.GymAdminPromotion,
            PriorityLevel = PriorityLevel.High,
            Status = RequestStatus.Submitted,
            Payload = null,
        };

        await AddAsync(request);

        var command = new FulfillOtherTypeRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfRequestIsNotInSubmittedStatus()
    {
        await RunAsAppAdminAsync();

        var request = new Request()
        {
            Title = "Title",
            Description = "Description",
            Type = RequestType.Other,
            PriorityLevel = PriorityLevel.High,
            Status = RequestStatus.Cancelled,
            Payload = null,
        };

        await AddAsync(request);

        var command = new FulfillOtherTypeRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldApproveRequest()
    {
        await RunAsAppAdminAsync();

        var request = new Request()
        {
            Title = "Title",
            Description = "Description",
            Type = RequestType.Other,
            PriorityLevel = PriorityLevel.High,
            Status = RequestStatus.Submitted,
            Payload = null,
        };

        await AddAsync(request);

        var command = new FulfillOtherTypeRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var updatedRequest = await FindAsync<Request>(request.Id);
        updatedRequest.ShouldNotBeNull();
        updatedRequest.Id.ShouldBe(request.Id);
        updatedRequest.Status.ShouldBe(RequestStatus.Approved);
    }
}
