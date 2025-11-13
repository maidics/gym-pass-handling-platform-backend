using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitPass.Application.ApplicationUsers.Commands.RoleHandling;
using FitPass.Application.Common.Exceptions;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands.RoleHandling;

using static Testing;

public class PromotePendingGymEmployeeToGymAdminFromRequestTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<PromotePendingGymEmployeeToGymAdminFromRequestCommand>(Roles.AppAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsAppAdminAsync();

        var command = new PromotePendingGymEmployeeToGymAdminFromRequestCommand(string.Empty);

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }
}
