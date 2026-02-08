using System.Text.Json;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Settings;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Requests.Commands.Fulfill;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.Tests.RequestTests.Commands.Fulfill;

using static Testing;

public class PromotePendingGymEmployeeToGymAdminFromRequestTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<PromotePendingGymEmployeeToGymAdminFromRequestCommand>(
            Roles.AppAdministrator
        );
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestNotFound()
    {
        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand("id");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnForbiddenWhenRequestIsNotSubmittedStatus()
    {
        var request = new Request
        {
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Cancelled,
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = null,
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Forbidden);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationWhenRequestIsNotOfGymAdminPromotionType()
    {
        var request = new Request
        {
            Type = RequestType.Other,
            Status = RequestStatus.Submitted,
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = null,
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
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Submitted,
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = "payload",
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.InternalError);

        var updatedRequest = await FindAsync<Request>(request.Id);
        updatedRequest.ShouldNotBeNull();
        updatedRequest.Status.ShouldBe(RequestStatus.Error);
        updatedRequest.Error.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserToNominateNotFound()
    {
        var request = new Request
        {
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Submitted,
            Title = "Title",
            Description = "Description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = JsonSerializer.Serialize(
                TestEntityBuilder.CreateGymAdminPromotionDto("GymId", "id"),
                JsonDefaults.SerializerOptions
            ),
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfUserToPromoteIsNotPendingGymEmployee()
    {
        var user = await CreateUserAsync();

        var request = new Request
        {
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Submitted,
            Title = "title",
            Description = "description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = JsonSerializer.Serialize(
                TestEntityBuilder.CreateGymAdminPromotionDto("GymId", user.Email!),
                JsonDefaults.SerializerOptions
            ),
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldReturnNotFoundWhenGymNotFound()
    {
        var pendingGymEmployee = await CreateUserAsync(role: Roles.PendingGymEmployee);

        var request = new Request
        {
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Submitted,
            Title = "title",
            Description = "description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = JsonSerializer.Serialize(
                TestEntityBuilder.CreateGymAdminPromotionDto(
                    "non-existent-gym-id",
                    pendingGymEmployee.Id
                ),
                JsonDefaults.SerializerOptions
            ),
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldPromotePendingGymEmployee()
    {
        var pendingGymEmployee = await CreateUserAsync(role: Roles.PendingGymEmployee);

        var obj = await TestEntityBuilder.BuildGymAsync();

        var request = new Request
        {
            Type = RequestType.GymAdminPromotion,
            Status = RequestStatus.Submitted,
            Title = "title",
            Description = "description",
            PriorityLevel = PriorityLevel.Medium,
            Payload = JsonSerializer.Serialize(
                TestEntityBuilder.CreateGymAdminPromotionDto(obj.gym.Id, pendingGymEmployee.Email!),
                JsonDefaults.SerializerOptions
            ),
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var gymAdmin = await FindAsync<ApplicationUser>(pendingGymEmployee.Id);
        gymAdmin.ShouldNotBeNull();
        var roles = await GetUserRolesAsync(gymAdmin.Id);
        roles.ShouldNotBeEmpty();
        roles.Count.ShouldBe(1);
        roles.ShouldContain(Roles.GymAdministrator);
    }
}
