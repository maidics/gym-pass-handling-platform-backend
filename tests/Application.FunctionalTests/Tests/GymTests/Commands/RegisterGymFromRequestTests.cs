using System.Text.Json;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Gyms.Commands;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.Tests.GymTests.Commands;

using static Testing;

public class RegisterGymFromRequestTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<RegisterGymFromRequestCommand>(Roles.AppAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(string.Empty);

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfRequestNotExists()
    {
        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand("notExistsId");

        await Should.ThrowAsync<NotFoundException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfRequestIsNoLongerSubmittedStatus()
    {
        var request = await RequestBuilder.WithRequestStatus(RequestStatus.Completed).BuildAsync();

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        await Should.ThrowAsync<ForbiddenAccessException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfRequestIsNotGymCreationType()
    {
        var request = await RequestBuilder.WithRequestType(RequestType.Other).BuildAsync();

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        await Should.ThrowAsync<BadRequestException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfRequestCreatorNotExists()
    {
        var request = await RequestBuilder
            .WithRequestType(RequestType.GymCreation)
            .BuildAsync();

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        await Should.ThrowAsync<ArgumentNullException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfPayloadIsNull()
    {
        var pendingGymEmployee = await ApplicationUserBuilder
            .WithRole(Roles.PendingGymEmployee)
            .BuildAsync();

        await RunAsUserAsync(pendingGymEmployee);

        var request = await RequestBuilder
            .WithRequestType(RequestType.GymCreation)
            .WithRequestStatus(RequestStatus.Submitted)
            .WithCreatedBy(pendingGymEmployee.Id)
            .BuildAsync();

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        await Should.ThrowAsync<ArgumentNullException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfPayloadFailsToDeserialize()
    {
        var pendingGymEmployee = await ApplicationUserBuilder
            .WithRole(Roles.PendingGymEmployee)
            .BuildAsync();

        await RunAsUserAsync(pendingGymEmployee);

        var request = await RequestBuilder
            .WithRequestType(RequestType.GymCreation)
            .WithRequestStatus(RequestStatus.Submitted)
            .WithCreatedBy(pendingGymEmployee.Id)
            .WithPayload("invalidPayload")
            .BuildAsync();

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        await Should.ThrowAsync<JsonException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfGymWithNameAlreadyExists()
    {
        var gym = await GymBuilder
            .WithName("Name")
            .BuildAsync();

        var pendingGymEmployee = await ApplicationUserBuilder
            .WithRole(Roles.PendingGymEmployee)
            .BuildAsync();

        await RunAsUserAsync(pendingGymEmployee);

        var createGymDto = new CreateGymDto
        {
            GymName = "Name",
            GymAddress = "Address",
            GymStatus = GymStatus.Active,
            GymTier = GymTier.Local,
            EscalationEmail = "valid@email"
        };

        var request = await RequestBuilder
            .WithRequestType(RequestType.GymCreation)
            .WithRequestStatus(RequestStatus.Submitted)
            .WithCreatedBy(pendingGymEmployee.Id)
            .WithPayload(createGymDto)
            .BuildAsync();

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        await Should.ThrowAsync<ConflictException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfRequestCreatorIsNotPendingGymEmployee()
    {
        var notPendingGymEmployee = await ApplicationUserBuilder
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        await RunAsUserAsync(notPendingGymEmployee);

        var request = await RequestBuilder
            .WithRequestType(RequestType.GymCreation)
            .WithRequestStatus(RequestStatus.Submitted)
            .WithCreatedBy(notPendingGymEmployee.Id)
            .BuildAsync();

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        await Should.ThrowAsync<BadRequestException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldCreateGym()
    {
        var obj = await TestEntityBuilder.BuildGymCreationRequest();

        await RunAsUserAsync(obj.pendingGymEmployee);

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(obj.request.Id);

        var gymDto = await SendAsync(command);
        gymDto.ShouldNotBeNull();

        var gymCount = await CountAsync<Gym>();
        gymCount.ShouldBe(1);
        var createdGym = await GetFirstAsync<Gym>();
        createdGym.ShouldNotBeNull();
        createdGym.Name.ShouldBe(obj.createGymDto.GymName);
        createdGym.Address.ShouldBe(obj.createGymDto.GymAddress);
        createdGym.Status.ShouldBe(obj.createGymDto.GymStatus);
        createdGym.Tier.ShouldBe(obj.createGymDto.GymTier);

        var nominatedGymAdmin = await FindAsync<ApplicationUser>(obj.pendingGymEmployee.Id);
        nominatedGymAdmin.ShouldNotBeNull();
        var roles = await GetUserRolesAsync(nominatedGymAdmin.Id);
        roles.Count.ShouldBe(1);
        roles.First().ShouldBe(Roles.GymAdministrator);

        var createdGymEmployment = await FindByApplicationUserIdAsync<GymEmployment>(obj.pendingGymEmployee.Id);
        createdGymEmployment.ShouldNotBeNull();
        createdGymEmployment.ApplicationUserId.ShouldBe(obj.pendingGymEmployee.Id);
        createdGymEmployment.GymId.ShouldBe(createdGym.Id);
        createdGymEmployment.Role.ShouldBe(Roles.GymAdministrator);
        createdGymEmployment.EscalationEmail.ShouldBe(obj.createGymDto.EscalationEmail);
    }
}
