using System.Text.Json;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
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
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsPendingGymEmployeeAsync();

        var command = new CreateGymCreationRequestCommand(
            string.Empty,
            string.Empty,
            PriorityLevel.Medium,
            new CreateGymDto
            {
                Name = string.Empty,
                Address = new Address("line1", "line2", "city", null, "postalCode", "HU"),
                Status = GymStatus.Suspended,
                Tier = GymTier.Local,
                SupervisorEmail = string.Empty,
            }
        );

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfUserEmailIsNotConfirmed()
    {
        await RunAsPendingGymEmployeeAsync();

        var command = new CreateGymCreationRequestCommand(
            "Title",
            "Description",
            PriorityLevel.High,
            TestEntityBuilder.BuildCreateGymDto()
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
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
            Payload = JsonSerializer.Serialize(TestEntityBuilder.BuildCreateGymDto()),
        };

        await AddAsync(request);

        var command = new CreateGymCreationRequestCommand(
            "Title",
            "Description",
            PriorityLevel.High,
            TestEntityBuilder.BuildCreateGymDto()
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldCreateGymCreationRequest()
    {
        var pendingGymEmployee = await CreateUserAsync(
            role: Roles.PendingGymEmployee,
            emailConfirmed: true
        );

        await RunAsUserAsync(pendingGymEmployee);

        var createGymDto = TestEntityBuilder.BuildCreateGymDto();

        var command = new CreateGymCreationRequestCommand(
            "Title",
            "Description",
            PriorityLevel.High,
            createGymDto
        );

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var dto = result.Value;

        var request = await GetFirstAsync<Request>();
        request.ShouldNotBeNull();
        request.Id.ShouldBe(dto.Id);
        request.Title.ShouldBe(command.Title);
        request.Description.ShouldBe(command.Description);
        request.PriorityLevel.ShouldBe(command.PriorityLevel);
        request.Status.ShouldBe(RequestStatus.Submitted);
        request.Payload.ShouldNotBeNull();
        request.Payload.ShouldBe(JsonSerializer.Serialize(createGymDto));
    }
}
