using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.Requests.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.RequestTests.Commands;

using static Testing;

public class CreatePayloadFreeRequestTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<CreatePayloadFreeRequestCommand>(
            Roles.User,
            Roles.PendingGymEmployee,
            Roles.GymStaff,
            Roles.GymAdministrator
        );
    }

    [TestCase("", "", PriorityLevel.None, RequestType.Other)]
    [TestCase("Title", "Description", PriorityLevel.None, RequestType.GymAdminPromotion)]
    [TestCase("Title", "Description", PriorityLevel.None, RequestType.GymCreation)]
    public async Task ShouldThrowIfParametersAreInvalid(
        string title,
        string description,
        PriorityLevel priorityLevel,
        RequestType requestType
    )
    {
        await RunAsDefaultUserAsync();

        var command = new CreatePayloadFreeRequestCommand(
            title,
            description,
            priorityLevel,
            requestType
        );

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldCreateRequest()
    {
        await RunAsDefaultUserAsync();

        var command = new CreatePayloadFreeRequestCommand(
            "Title",
            "Description",
            PriorityLevel.High,
            RequestType.Other
        );

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var request = await GetFirstAsync<Request>();
        request.ShouldNotBeNull();
        request.Title.ShouldBe(command.Title);
        request.Description.ShouldBe(command.Description);
        request.PriorityLevel.ShouldBe(command.PriorityLevel);
        request.Type.ShouldBe(command.RequestType);
        request.Payload.ShouldBeNull();
    }
}
