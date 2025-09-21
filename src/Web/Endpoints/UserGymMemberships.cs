using Fitpass.Application.UserGymMemberships.Commands;
using FitPass.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fitpass.Web.Endpoints;

public class UserGymMemberships : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut("/{applicationUserId}/{gymId}", UpdateUserMembershipStatus).RequireAuthorization(); //TODO: fix this so it takes user gym membership id
    }

    public async Task<Results<Ok<Result>, BadRequest>> UpdateUserMembershipStatus(ISender sender, string applicationUserId, string gymId, [AsParameters] UpdateUserGymMembershipStatusCommand command)
    {
        if (applicationUserId != command.ApplicationUserId || gymId != command.GymId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }
}
