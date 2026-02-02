using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymEmployments.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymEmploymentTests.Queries;

using static Testing;

public class GetMyGymEmploymentsTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetMyGymEmploymentsQuery>(
            Roles.GymAdministrator,
            Roles.GymStaff
        );
    }

    [Test]
    public async Task ShouldReturnGymEmployments()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var gymEmployments = await SendAsync(new GetMyGymEmploymentsQuery());
        gymEmployments
            .Count(x =>
                x.Id == obj.gymStaffGymEmployment.Id || x.Id == obj.gymAdminGymEmployment.Id
            )
            .ShouldBe(2);
    }
}
