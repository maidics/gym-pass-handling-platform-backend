using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymMemberships.Commands;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.GymMembershipTests.Commands;

using static Testing;

public class GetOrCreateGymMembershipTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<GetOrCreateGymMembershipCommand>();
    }

    [Test]
    public async Task ShouldReturnGymMembershipIfExists()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var command = new GetOrCreateGymMembershipCommand(obj.gymMember.Id, obj.gym.Id);

        var result = await SendAsync(command);
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(command.UserId);
        result.GymId.ShouldBe(command.GymId);
    }

    [Test]
    public async Task ShouldCreateGymMembershipIfNotExists()
    {
        var user = await CreateUserAsync();

        var obj = await TestEntityBuilder.BuildGymAsync();

        var command = new GetOrCreateGymMembershipCommand(user.Id, obj.gym.Id);

        var gymMembershipCount = await CountAsync<GymMembership>();
        gymMembershipCount.ShouldBe(1);

        var result = await SendAsync(command);
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(command.UserId);
        result.GymId.ShouldBe(command.GymId);

        gymMembershipCount = await CountAsync<GymMembership>();
        gymMembershipCount.ShouldBe(2);

        var createdGymMembership = await FindAsync<GymMembership>(result.Id);
        createdGymMembership.ShouldNotBeNull();
        createdGymMembership.UserId.ShouldBe(command.UserId);
        createdGymMembership.GymId.ShouldBe(command.GymId);
    }
}
