using Fitpass.Application.Common.Exceptions;
using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.ApplicationUsers.Commands;

using static Testing;
public class NominateGymStaffTests
{
    [Test]
    public async Task ShouldNominatePendingGymManagementUserToGymStaff()
    {
        var pendingGymManagementUser = await TestApplicationUserBuilder.WithRole(Roles.PendingGymEmployee).BuildAsync();

        var gymBuilder = TestGymBuilder;

        await gymBuilder.WithGymAdmin().BuildAsync();

        var nominatorGymAdmin = gymBuilder.GetGymAdmin();

        await RunAsUserAsync(nominatorGymAdmin);

        var escalationEmail = "escalation@email";

        var command = new NominateGymStaffCommand(pendingGymManagementUser.Email!, escalationEmail);

        var action = () => SendAsync(command);

        await action.ShouldNotThrowAsync();

        var nominatedUser = await FindAsync<ApplicationUser>([pendingGymManagementUser.Id], u => u.GymStaffAssignment);

        nominatedUser.ShouldNotBeNull();
        nominatedUser.GymStaffAssignment.ShouldNotBeNull();
        nominatedUser.GymStaffAssignment.Role.ShouldBe(Roles.GymStaff);
        nominatedUser.GymStaffAssignment.GymId.ShouldBe(nominatorGymAdmin.GymStaffAssignment!.GymId);
        nominatedUser.GymStaffAssignment.EscalationEmail.ShouldBe(escalationEmail);

        var nominatedUserRoles = await GetUserRoleAsync(nominatedUser);

        nominatedUserRoles.Count().ShouldBe(1);
        nominatedUserRoles.First().ShouldBe(Roles.GymStaff);
    }

    [Test]
    public async Task ShouldDenyNonGymAdministratorUser()
    {
        var command = new NominateGymStaffCommand("user@email", "escalation@email");
        var action = () => SendAsync(command);

        command.GetType().ShouldSatisfyAllConditions(type => type.ShouldBeDecoratedWith<AuthorizeAttribute>());

        await RunAsDefaultUserAsync();
        await action.ShouldThrowAsync<ForbiddenAccessException>();

        await RunAsAppAdminAsync();
        await action.ShouldThrowAsync<ForbiddenAccessException>();

        await RunAsGymStaffAsync();
        await action.ShouldThrowAsync<ForbiddenAccessException>();

        await RunAsPendingGymManagementAsync();
        await action.ShouldThrowAsync<ForbiddenAccessException>();

        await ResetState();
        await action.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task ShouldThrowWithNotValidParameters()
    {
        var command = new NominateGymStaffCommand(string.Empty, string.Empty);
        var action = () => SendAsync(command);

        await RunAsGymAdminAsync();

        var ex = await action.ShouldThrowAsync<ValidationException>();

        ex.Errors.ShouldContainKey("UserEmailToNominate");
        ex.Errors["UserEmailToNominate"].ShouldContain("User's email to nominate is required.");
        ex.Errors["UserEmailToNominate"].ShouldContain("Valid email address is required.");
        ex.Errors.ShouldContainKey("EscalationEmail");
        ex.Errors["EscalationEmail"].ShouldContain("Escalation email is required.");
        ex.Errors["EscalationEmail"].ShouldContain("Valid email address is required.");
        ex.Errors["EscalationEmail"].ShouldContain("'Escalation Email' must not be equal to ''.");
    }

    [Test]
    public async Task ShouldDenyNominationToNonPendingGymManagementUser()
    {
        var defaultUser = await TestApplicationUserBuilder.BuildAsync();

        var command = new NominateGymStaffCommand(defaultUser.Email!, "escalation@email");
        var action = () => SendAsync(command);

        var gymBuilder = TestGymBuilder;

        await gymBuilder.WithGymAdmin().BuildAsync();

        var nominatorGymAdmin = gymBuilder.GetGymAdmin();

        await RunAsUserAsync(nominatorGymAdmin);


        var ex = await action.ShouldThrowAsync<BadRequestException>();

        ex.Message.ShouldBe("Account with this email is not eligible for GymStaff nomination. Please register a new gym management account for this action");
    }
}
