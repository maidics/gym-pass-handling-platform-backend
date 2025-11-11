using System.Text.Json;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Requests.Commands;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

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
                GymAddress = string.Empty,
                GymStatus = GymStatus.Suspended,
                GymTier = GymTier.Local,
                EscalationEmail = string.Empty
            });

        await ShouldThrowIfParametersAreInvalid(command);
    }

    [Test]
    public async Task ShouldThrowIfUserEmailIsNotConfirmed()
    {
        var user = await ApplicationUserBuilder
            .WithRole(Roles.PendingGymEmployee)
            .BuildAsync();

        await RunAsUserAsync(user);

        var command = new CreateGymCreationRequestCommand("Description", PriorityLevel.High, TestEntityBuilder.BuildCreateGymDto());

        var ex = await Should.ThrowAsync<BadRequestException>(SendAsync(command));
        ex.Message.ShouldBe("You must confirm your email before this action.");
    }

    [Test]
    public async Task ShouldThrowIfUserAlreadyHasAGymCreationRequest()
    {
        var obj = await RunAsPendingGymEmployeeAsync();

        await RequestBuilder
            .WithCreatedBy(obj.user.Id)
            .WithRequestType(RequestType.GymCreation)
            .WithPayload(TestEntityBuilder.BuildCreateGymDto())
            .BuildAsync();

        var command = new CreateGymCreationRequestCommand("Description", PriorityLevel.High, TestEntityBuilder.BuildCreateGymDto());

        var ex = await Should.ThrowAsync<BadRequestException>(SendAsync(command));
        ex.Message.ShouldBe("You already have an ongoing gym creation request.");
    }

    [Test]
    public async Task ShouldCreateGymCreationRequest()
    {
        var obj = await RunAsPendingGymEmployeeAsync();

        var createGymDto = TestEntityBuilder.BuildCreateGymDto();

        var command = new CreateGymCreationRequestCommand("Description", PriorityLevel.High, createGymDto);

        await Should.NotThrowAsync(SendAsync(command));

        var request = await GetFirstAsync<Request>();
        request.ShouldNotBeNull();
        request.Description.ShouldBe("Description");
        request.PriorityLevel.ShouldBe(PriorityLevel.High);
        request.Status.ShouldBe(RequestStatus.Submitted);
        request.Title.ShouldBe($"'{command.CreateGymDto.GymName}' gym creation");
        request.Payload.ShouldNotBeNull();
        request.Payload.ShouldBe(JsonSerializer.Serialize(createGymDto));

        //cannot check createdBy because interceptor is not injected
    }
}
