using Fitpass.Application.ApplicationUsers.Commands;
using Fitpass.Application.ApplicationUsers.Queries;
using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Models;
using FitPass.Application.Gyms.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fitpass.Web.Endpoints.Users;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(RegisterUser, "Register");

        groupBuilder.MapPost(LogInUser, "LogIn");

        groupBuilder.MapPost(RegisterPendingGymManagement, "Register/PendingGymManagement");

        groupBuilder.MapGet(GetAllMyGymStaff, "GymManagement/My").RequireAuthorization();

        groupBuilder.MapPut(NominateGymStaff, "Nominate/GymStaff").RequireAuthorization();

        groupBuilder.MapGet(GetMyGymManagementUsers, "GymManagement/My/GymMembers").RequireAuthorization();

        groupBuilder.MapGet(GetGymManagementUsers, "Management/{gymId}").RequireAuthorization();

        groupBuilder.MapDelete(DeleteMyAccount, "My/Delete").RequireAuthorization();
    }

    public async Task<Ok<string>> RegisterUser(ISender sender, [FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<string>> LogInUser(ISender sender, [FromBody] LogInUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<string>> RegisterPendingGymManagement(ISender sender, [FromBody] RegisterPendingGymManagementCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<ApplicationUserDto>>> GetAllMyGymStaff(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllMyGymStaffQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok> NominateGymStaff(ISender sender, [FromBody] NominateGymStaffCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.Ok();
    }

    public async Task<Ok<List<ApplicationUserDto>>> GetMyGymManagementUsers(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyGymManagementUsersQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<ApplicationUserDto>>> GetGymManagementUsers(ISender sender, [FromRoute] string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymStaffQuery(gymId), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok> DeleteMyAccount(ISender sender)
    {
        await sender.Send(new DeleteMyAccountCommand());

        return TypedResults.Ok();
    }
}
