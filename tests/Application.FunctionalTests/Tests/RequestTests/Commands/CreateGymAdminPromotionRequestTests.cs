using System.Text.Json;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.Requests.Commands;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.RequestTests.Commands;

using static Testing;

public class CreateGymAdminPromotionRequestTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<CreateGymAdminPromotionRequestCommand>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new CreateGymAdminPromotionRequestCommand(
            string.Empty,
            string.Empty,
            string.Empty,
            PriorityLevel.High,
            string.Empty
        );

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserToPromoteIsNotFound()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new CreateGymAdminPromotionRequestCommand(
            "Title",
            "userId",
            "Description",
            PriorityLevel.Medium,
            "supervisor@test.com"
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfTheUserToPromoteIsNotPendingGymEmployee()
    {
        var user = await CreateUserAsync();

        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new CreateGymAdminPromotionRequestCommand(
            "Title",
            user.Id,
            "Description",
            PriorityLevel.High,
            "escalation@email"
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldCreateRequest()
    {
        var pendingGymEmployee = await CreateUserAsync(role: Roles.PendingGymEmployee);

        var gymAdminObj = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new CreateGymAdminPromotionRequestCommand(
            "Title",
            pendingGymEmployee.Id,
            "Description",
            PriorityLevel.High,
            "escalation@email"
        );

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var createdRequest = await GetFirstAsync<Request>();
        createdRequest.ShouldNotBeNull();
        createdRequest.Title.ShouldBe(command.Title);
        createdRequest.Type.ShouldBe(RequestType.GymAdminPromotion);
        createdRequest.Description.ShouldBe(command.Description);
        createdRequest.PriorityLevel.ShouldBe(PriorityLevel.High);
        createdRequest.Payload.ShouldNotBeNull();

        var payload = JsonSerializer.Deserialize<GymAdminPromotionDto>(createdRequest.Payload);
        payload.ShouldNotBeNull();
        payload.GymId.ShouldBe(gymAdminObj.gymEmployment.GymId);
        payload.PendingGymEmployeeEmail.ShouldBe(pendingGymEmployee.Id);
        payload.SupervisorEmail.ShouldBe(command.SupervisorEmail);
    }
}
