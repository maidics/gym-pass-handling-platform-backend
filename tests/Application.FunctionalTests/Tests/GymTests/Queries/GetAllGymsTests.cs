using System;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Gyms.Queries;

namespace FitPass.Application.FunctionalTests.Tests.GymTests.Queries;

using static Testing;

public class GetAllGymsTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<GetAllGymsQuery>();
    }

    [Test]
    public async Task ShouldReturnGyms()
    {
        var obj1 = await TestEntityBuilder.BuildGymAsync();
        var obj2 = await TestEntityBuilder.BuildGymAsync();
        var obj3 = await TestEntityBuilder.BuildGymAsync();

        var command = new GetAllGymsQuery();

        var gymDtos = await SendAsync(command);
        gymDtos.ShouldNotBeNull();
        gymDtos.Count.ShouldBe(3);
        gymDtos.FirstOrDefault(g => g.Id == obj1.gym.Id)
            .ShouldNotBeNull()
            .AssertToGym(obj1.gym);

        gymDtos.FirstOrDefault(g => g.Id == obj2.gym.Id)
            .ShouldNotBeNull()
            .AssertToGym(obj2.gym);

        gymDtos.FirstOrDefault(g => g.Id == obj3.gym.Id)
            .ShouldNotBeNull()
            .AssertToGym(obj3.gym);
    }
}
