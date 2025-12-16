using System.Text.Json;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Requests.Commands.Fulfill;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands.RoleHandling;

using static Testing;

public class PromotePendingGymEmployeeToGymAdminFromRequestTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<PromotePendingGymEmployeeToGymAdminFromRequestCommand>(Roles.AppAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(string.Empty);

        await Should.ThrowAsync<Common.Exceptions.ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestNotFound()
    {
        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand("non-existent-request-id");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnForbiddenWhenRequestIsNotSubmittedStatus()
    {
        var request = new Request
        {
            Id = "request-id",
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Cancelled,
            Title = "title",
            Description = "description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = null
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Forbidden);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationWhenRequestIsNotOfGymAdminPromotionType()
    {
        var request = new Request
        {
            Id = "request-id",
            Type = RequestType.Other,
            Status = RequestStatus.Submitted,
            Title = "title",
            Description = "description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = null
        };
        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnInternalErrorWhenPayloadFailsToSerialize()
    {
        var request = new Request
        {
            Id = "request-id",
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Submitted,
            Title = "title",
            Description = "description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = "invalid-payload"
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.InternalError);
        result.Message.ShouldNotBeEmpty();

        request = await FindAsync<Request>(request.Id);
        request.ShouldNotBeNull();
        request.Status.ShouldBe(RequestStatus.Error);
        request.Error.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldReturNotFoundWhenUserToNominateNotFound()
    {
        var request = new Request
        {
            Id = "request-id",
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Submitted,
            Title = "title",
            Description = "description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = JsonSerializer.Serialize(TestEntityBuilder.CreateGymAdminPromotionDto("gymId", "not-existent-user-id"))
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfUserToPromoteIsNotPendingGymEmployee()
    {
        var defaultUser = await CreateUserAsync();

        var request = new Request
        {
            Id = "request-id",
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Submitted,
            Title = "title",
            Description = "description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = JsonSerializer.Serialize(TestEntityBuilder.CreateGymAdminPromotionDto("gymId", defaultUser.Id))
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnNotFoundWhenGymNotFound()
    {
        var pendingGymEmployee = await CreateUserAsync(role: Roles.PendingGymEmployee);

        var request = new Request
        {
            Id = "request-id",
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Submitted,
            Title = "title",
            Description = "description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = JsonSerializer.Serialize(TestEntityBuilder.CreateGymAdminPromotionDto("non-existent-gym-id", pendingGymEmployee.Id))
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldPromotePendingGymEmployee()
    {
        var pendingGymEmployee = await CreateUserAsync(role: Roles.PendingGymEmployee);

        var obj = await TestEntityBuilder.BuildGymAsync();

        var request = new Request
        {
            Id = "request-id",
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Submitted,
            Title = "title",
            Description = "description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = JsonSerializer.Serialize(TestEntityBuilder.CreateGymAdminPromotionDto(obj.gym.Id, pendingGymEmployee.Id))
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var gymAdmin = await FindAsync<ApplicationUser>(pendingGymEmployee.Id);
        gymAdmin.ShouldNotBeNull();
        var roles = await GetUserRolesAsync(gymAdmin.Id);
        roles.ShouldNotBeEmpty();
        roles.Count.ShouldBe(1);
        roles.ShouldContain(Roles.GymAdministrator);
    }
}
