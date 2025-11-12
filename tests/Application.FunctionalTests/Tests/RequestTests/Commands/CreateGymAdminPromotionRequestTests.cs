using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitPass.Application.Common.Exceptions;
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
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new CreateGymAdminPromotionRequestCommand(
            string.Empty,
            string.Empty,
            PriorityLevel.High,
            string.Empty);

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfUserToPromoteDoesNotExist()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new CreateGymAdminPromotionRequestCommand(
            "invalidUserId", 
            "Description", 
            PriorityLevel.Medium, 
            "email@email");
    }

    [Test]
    public async Task ShouldThrowIfUserToPromoteIsNotInPendingGymEmployeeRole()
    {
        var user = await ApplicationUserBuilder.BuildAsync();

        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new CreateGymAdminPromotionRequestCommand(
            user.Id,
            "Description",
            PriorityLevel.High,
            "escalation@email");

        await Should.ThrowAsync<BadRequestException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldCreateRequest()
    {
        var pendingGymEmployee = await ApplicationUserBuilder
            .WithRole(Roles.PendingGymEmployee)
            .BuildAsync();

        var gymAdminObj = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new CreateGymAdminPromotionRequestCommand(
            pendingGymEmployee.Id,
            "Description",
            PriorityLevel.High,
            "escalation@email");

        await SendAsync(command);

        var createdRequest = await GetFirstAsync<Request>();
        createdRequest.ShouldNotBeNull();
        createdRequest.Type.ShouldBe(RequestType.GymAdminPromotion);
        createdRequest.Description.ShouldBe(command.RequestDescription);
        createdRequest.PriorityLevel.ShouldBe(PriorityLevel.High);
        var payload = createdRequest.DeserializePayload<GymAdminPromotionDto>();
        payload.ShouldNotBeNull();
        payload.GymId.ShouldBe(gymAdminObj.gymEmployment.GymId);
        payload.UserIdToNominate.ShouldBe(pendingGymEmployee.Id);
        payload.EscalationEmail.ShouldBe(command.EscalationEmail);
    }
}
