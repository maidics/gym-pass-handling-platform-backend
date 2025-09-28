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

        groupBuilder.MapPost(RegisterGymAdministratorUser, "Register/GymAdministrator").RequireAuthorization();

        groupBuilder.MapGet(GetAllMyGymStaff, "GymManagement/My").RequireAuthorization();

        groupBuilder.MapPut(NominateUserToGymStaffMember, "Nominate/GymStaff").RequireAuthorization();

        groupBuilder.MapGet(GetMyGymManagementUsers, "GymManagement/My/GymMembers").RequireAuthorization();

        groupBuilder.MapGet(GetGymManagementUsers, "Management/{gymId}").RequireAuthorization();

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

    public async Task<Ok<Result>> RegisterGymAdministratorUser(ISender sender, [AsParameters] RegisterGymAdministratorUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<ApplicationUserDto>>> GetAllMyGymStaff(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllMyGymStaffQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok> NominateUserToGymStaffMember(ISender sender, [FromBody] NominateUserToGymStaffMemberCommand command, CancellationToken cancellationToken)
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
}
