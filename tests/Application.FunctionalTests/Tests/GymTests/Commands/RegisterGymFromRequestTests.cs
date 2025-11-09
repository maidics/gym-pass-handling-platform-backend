using FitPass.Application.Gyms.Commands;
using FitPass.Domain.Constants;

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

    }
}
