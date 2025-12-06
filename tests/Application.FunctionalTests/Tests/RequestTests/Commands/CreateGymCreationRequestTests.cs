using System.Text.Json;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Requests.Commands;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.FunctionalTests.Tests.RequestTests.Commands;

using static Testing;

public class CreateGymCreationRequestTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<CreateGymCreationRequestCommand>(Roles.PendingGymEmployee);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsPendingGymEmployeeAsync();

        var command = new CreateGymCreationRequestCommand(
            string.Empty,
            PriorityLevel.Medium,
            new CreateGymDto
            {
                GymName = string.Empty,
                GymAddress = new Address("line1", "line2", "city", null, "postalCode", "HU"),
                GymStatus = GymStatus.Suspended,
                GymTier = GymTier.Local,
                EscalationEmail = string.Empty
            });

        await ShouldThrowIfParametersAreInvalid(command);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfUserEmailIsNotConfirmed()
    {
        await RunAsPendingGymEmployeeAsync();

        var command = new CreateGymCreationRequestCommand("Description", PriorityLevel.High, TestEntityBuilder.BuildCreateGymDto());

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldContain("You must confirm your email before this action.");
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfUserAlreadyHasAGymCreationRequest()
    {
        var obj = await RunAsPendingGymEmployeeAsync(true);

        var request = new Request
        {
            Title = "Gym Creation Request",
            CreatedBy = obj.user.Id,
            Type = RequestType.GymCreation,
            Description = "Existing request",
            PriorityLevel = PriorityLevel.Medium,
            Status = RequestStatus.Submitted,
            Payload = JsonSerializer.Serialize(TestEntityBuilder.BuildCreateGymDto())
        };

        await AddAsync(request);

        var command = new CreateGymCreationRequestCommand("Description", PriorityLevel.High, TestEntityBuilder.BuildCreateGymDto());

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldContain("You already have an ongoing gym creation request.");
    }

    [Test]
    public async Task ShouldCreateGymCreationRequest()
    {
        var pendingGymEmployee = await CreateUserAsync(role: Roles.PendingGymEmployee, emailConfirmed: true);

        await RunAsUserAsync(pendingGymEmployee);

        var createGymDto = TestEntityBuilder.BuildCreateGymDto();

        var command = new CreateGymCreationRequestCommand("Description", PriorityLevel.High, createGymDto);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var request = await GetFirstAsync<Request>();
        request.ShouldNotBeNull();
        request.Description.ShouldBe("Description");
        request.PriorityLevel.ShouldBe(PriorityLevel.High);
        request.Status.ShouldBe(RequestStatus.Submitted);
        request.Payload.ShouldNotBeNull();
        request.Payload.ShouldBe(JsonSerializer.Serialize(createGymDto));
    }
}
