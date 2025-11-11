using System;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Gyms.Commands;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymTests.Commands;

using static Testing;

public class RegisterGymTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<RegisterGymCommand>(Roles.AppAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsAppAdminAsync();

        var command = new RegisterGymCommand(
            new CreateGymDto
            {
                GymName = string.Empty,
                GymAddress = string.Empty,
                GymStatus = GymStatus.Suspended,
                GymTier = GymTier.Local,
                EscalationEmail = string.Empty
            },
            string.Empty);

        await ShouldThrowIfParametersAreInvalid(command);
    }

    [Test]
    public async Task ShouldThrowIfGymNameInUse()
    {
        await RunAsAppAdminAsync();

        var obj = await TestEntityBuilder.BuildGymAsync();

        var createGymDto = TestEntityBuilder.BuildCreateGymDto();
        createGymDto.GymName = obj.gym.Name;

        var command = new RegisterGymCommand(createGymDto, "address@email");

        await Should.ThrowAsync<ConflictException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfUserToPromoteNoExists()
    {
        await RunAsAppAdminAsync();

        var createGymDto = TestEntityBuilder.BuildCreateGymDto();

        var command = new RegisterGymCommand(createGymDto, "invalid@email");

        await ShouldThrowIfNotFound(command);
    }

    [Test]
    public async Task ShouldThrowIfUserIsNotPendingGymEmployee()
    {
        await RunAsAppAdminAsync();

        var obj = await TestEntityBuilder.BuildGymEmployeeAsync(Roles.GymStaff);

        var createGymDto = TestEntityBuilder.BuildCreateGymDto();

        var command = new RegisterGymCommand(createGymDto, obj.user.Email!);

        await Should.ThrowAsync<BadRequestException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldRegisterGym()
    {
        await RunAsAppAdminAsync();

        var obj = await TestEntityBuilder.BuildPendingGymEmployeeAsync();

        var createGymDto = TestEntityBuilder.BuildCreateGymDto();

        var command = new RegisterGymCommand(createGymDto, obj.user.Email!);

        var gymDto = await SendAsync(command);
        gymDto.AssertToCreateGymDto(createGymDto);

        var gym = await GetFirstAsync<Gym>();
        gym.ShouldNotBeNull();
        gym.AssertToDto(gymDto);

        var gymEmployment = await FindByApplicationUserIdAsync<GymEmployment>(obj.user.Id);
        gymEmployment.ShouldNotBeNull();
        gymEmployment.ApplicationUserId.ShouldBe(obj.user.Id);
        gymEmployment.GymId.ShouldBe(gymDto.Id);
        gymEmployment.Role.ShouldBe(Roles.GymAdministrator);
    }
}
