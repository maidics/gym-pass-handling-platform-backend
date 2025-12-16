using System.Text.Json;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Requests.Commands.Fulfill;
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

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfRequestIsNotFound()
    {
        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand("notExistsId");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
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
        result.Message.ShouldNotBeEmpty();
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
        result.Message.ShouldNotBeEmpty();
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
            CreatedBy = null
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.InternalError);
        result.Message.ShouldNotBeEmpty();

        request = await FindAsync<Request>(request.Id);
        request.ShouldNotBeNull();
        request.Status.ShouldBe(RequestStatus.Error);
        request.Error.ShouldNotBeNull();
        request.Error.ShouldContain("Request creator is empty");
    }

    //not tested because of foreign key constraint on Request.CreatedBy
    /*
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
        result.Message.ShouldNotBeEmpty();

        request = await FindAsync<Request>(request.Id);
        request.ShouldNotBeNull();
        request.Status.ShouldBe(RequestStatus.Error);
        request.Error.ShouldBe("Request creator not found.");
    }
    */

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

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();

        request = await FindAsync<Request>(request.Id);
        request.ShouldNotBeNull();
        request.Status.ShouldBe(RequestStatus.Error);
        request.Error.ShouldBe("Request creator is no longer eligible for request completion.");
    }

    [Test]
    public async Task ShouldReturnInternalErrorIfPayloadFailsToDeserialize()
    {
        var pendingGymEmployee = await CreateUserAsync(role: Roles.PendingGymEmployee);

        var request = new Request
        {
            Title = "Test Request",
            Description = "Test Description",
            Type = RequestType.GymCreation,
            PriorityLevel = PriorityLevel.Medium,
            Status = RequestStatus.Submitted,
            Payload = "invalidPayload",
            CreatedBy = pendingGymEmployee.Id
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.InternalError);
        result.Message.ShouldNotBeEmpty();
        
        request = await FindAsync<Request>(request.Id);
        request.ShouldNotBeNull();
        request.Status.ShouldBe(RequestStatus.Error);
        request.Error.ShouldNotBeNull();
        request.Error.ShouldContain("Failed to deserialize payload.");
    }

    [Test]
    public async Task ShouldThrowIfGymWithNameAlreadyExists()
    {
        var pendingGymEmployee = await CreateUserAsync(role: Roles.PendingGymEmployee);

        var obj = await TestEntityBuilder.BuildGymAsync();

        var createGymDto = new CreateGymDto
        {
            Name = obj.gym.Name,
            Address = obj.gym.Address,
            Status = GymStatus.Active,
            Tier = GymTier.Local,
            EscalationEmail = "test@localhost"
        };

        var request = new Request
        {
            Title = "Test Request",
            Description = "Test Description",
            Type = RequestType.GymCreation,
            PriorityLevel = PriorityLevel.Medium,
            Status = RequestStatus.Submitted,
            Payload = JsonSerializer.Serialize(createGymDto),
            CreatedBy = pendingGymEmployee.Id
        };

        await AddAsync(request);

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(request.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Conflict);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldCreateGym()
    {
        var obj = await TestEntityBuilder.BuildGymCreationRequest();

        await RunAsAppAdminAsync();

        var command = new RegisterGymFromRequestCommand(obj.request.Id);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var gymCount = await CountAsync<Gym>();
        gymCount.ShouldBe(1);
        var createdGym = await GetFirstAsync<Gym>();
        createdGym.ShouldNotBeNull();
        createdGym.Name.ShouldBe(obj.createGymDto.Name);
        createdGym.Address.ShouldBe(obj.createGymDto.Address);
        createdGym.Status.ShouldBe(obj.createGymDto.Status);
        createdGym.Tier.ShouldBe(obj.createGymDto.Tier);

        var nominatedGymAdmin = await FindAsync<ApplicationUser>(obj.pendingGymEmployee.Id);
        nominatedGymAdmin.ShouldNotBeNull();
        var roles = await GetUserRolesAsync(nominatedGymAdmin.Id);
        roles.Count.ShouldBe(1);
        roles.First().ShouldBe(Roles.GymAdministrator);

        var createdGymEmployment = await FindByUserIdAsync<GymEmployment>(obj.pendingGymEmployee.Id);
        createdGymEmployment.ShouldNotBeNull();
        createdGymEmployment.UserId.ShouldBe(obj.pendingGymEmployee.Id);
        createdGymEmployment.GymId.ShouldBe(createdGym.Id);
        createdGymEmployment.Role.ShouldBe(Roles.GymAdministrator);
        createdGymEmployment.EscalationEmail.ShouldBe(obj.createGymDto.EscalationEmail);
    }
}
