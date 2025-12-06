using System;
using FitPass.Application.Gyms.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymTests.Queries;

using static Testing;

public class GetMyGymTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetMyGymQuery>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldReturnGym()
    {
        var obj = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var query = new GetMyGymQuery();

        var gymDto = await SendAsync(query);

        gymDto.AssertToGym(obj.gym);
    }
}
