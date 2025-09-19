using Fitpass.Application.ApplicationUsers.Commands;
using Fitpass.Application.ApplicationUsers.Queries;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fitpass.Web.Endpoints.ApplicationUsers;

public class ApplicationUsers : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetGymStaff).RequireAuthorization();

        groupBuilder.MapPost(RegisterGymAdministratorUser).RequireAuthorization();
    }

    public async Task<Results<Ok<List<ApplicationUserDto>>, BadRequest<string>>> GetGymStaff(ISender sender, [AsParameters] GetGymStaffQuery query)
    {
        var result = await sender.Send(query);

        if (result.errorMessage != null)
        {
            return TypedResults.BadRequest(result.errorMessage);
        }

        return TypedResults.Ok(result.gymStaffManagementUsers);
    }

    public async Task<Ok<Result>> RegisterGymAdministratorUser(ISender sender, [AsParameters] RegisterGymAdministratorUserCommand command)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }
}