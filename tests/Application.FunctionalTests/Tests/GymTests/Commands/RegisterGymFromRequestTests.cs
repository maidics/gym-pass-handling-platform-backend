using System.Text.Json;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
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

        await ShouldThrowIfParametersAreInvalid(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestIsNotFound()
    {
        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand("notExistsId");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldContain($"{nameof(Request)} not found");
    }

    [Test]
    public async Task ShouldReturnFobiddenIfRequestIsNoLongerSubmittedStatus()
    {
        var request = new Request
        {
            Title = "Test Request",
            Description = "Test Description",
            Type = RequestType.GymCreation,
            PriorityLevel = PriorityLevel.Medium,
            Status = RequestStatus.Completed,
            Payload = null
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Forbidden);
        result.Message.ShouldBe("Request is no longer open.");
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfRequestIsNotGymCreationType()
    {
        var request = new Request
        {
            Title = "Test Request",
            Description = "Test Description",
            Type = RequestType.Other,
            PriorityLevel = PriorityLevel.Medium,
            Status = RequestStatus.Submitted,
            Payload = null
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldBe("Request is not of GymCreation type.");
    }

    [Test]
    public async Task ShouldReturnInternalErrorIfRequestCreatorIsNull()
    {
        var request = new Request
        {
            Title = "Test Request",
            Description = "Test Description",
            Type = RequestType.GymCreation,
            PriorityLevel = PriorityLevel.Medium,
            Status = RequestStatus.Submitted,
            Payload = null,
            CreatedBy = "notExistsUserId"
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.InternalError);
        result.Message.ShouldBe("Request creator is empty.");

        request = await FindAsync<Request>(request.Id);
        request.ShouldNotBeNull();
        request.Status.ShouldBe(RequestStatus.Error);
        request.Error.ShouldBe("Request creator is empty.");
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestCreatorIsNotFound()
    {
        var request = new Request
        {
            Title = "Test Request",
            Description = "Test Description",
            Type = RequestType.GymCreation,
            PriorityLevel = PriorityLevel.Medium,
            Status = RequestStatus.Submitted,
            Payload = null,
            CreatedBy = "notExistsUserId"
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldBe($"{nameof(ApplicationUser)} not found");

        request = await FindAsync<Request>(request.Id);
        request.ShouldNotBeNull();
        request.Status.ShouldBe(RequestStatus.Error);
        request.Error.ShouldBe("Request creator not found.");
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfRequestCreatorIsNoLongerPendingGymEmployee()
    {
        var user = await CreateUserAsync(role: Roles.GymAdministrator);

        await RunAsAppAdminAsync();

        var request = new Request
        {
            Title = "Test Request",
            Description = "Test Description",
            Type = RequestType.GymCreation,
            PriorityLevel = PriorityLevel.Medium,
            Status = RequestStatus.Submitted,
            Payload = null,
            CreatedBy = user.Id
        };

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldContain("User is no longer a PendingGymEmployee");

        request = await FindAsync<Request>(request.Id);
        request.ShouldNotBeNull();
        request.Status.ShouldBe(RequestStatus.Error);
        request.Error.ShouldBe("Request creator is no longer eligible for request completion.");
    }

    [Test]
    public async Task ShouldReturnInternalErrorIfPayloadFailsToDeserialize()
    {
        throw new NotImplementedException();

        var pendingGymEmployee = await CreateUserAsync(role: Roles.PendingGymEmployee);

        await RunAsUserAsync(pendingGymEmployee);

        var request = await RequestBuilder
            .WithRequestType(RequestType.GymCreation)
            .WithRequestStatus(RequestStatus.Submitted)
            .WithCreatedBy(pendingGymEmployee.Id)
            .WithPayload("invalidPayload")
            .BuildAsync();

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        await Should.ThrowAsync<ArgumentException>(SendAsync(command));
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
        createdGymEmployment.UserId.ShouldBe(obj.pendingGymEmployee.Id);
        createdGymEmployment.GymId.ShouldBe(createdGym.Id);
        createdGymEmployment.Role.ShouldBe(Roles.GymAdministrator);
        createdGymEmployment.EscalationEmail.ShouldBe(obj.createGymDto.EscalationEmail);
    }
}
