using Fitpass.Application.UserGymMemberships.Commands;
using FitPass.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fitpass.Web.Endpoints;

public class UserGymMemberships : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut("/{applicationUserId:string}/{gymId:string}", UpdateUserMembershipStatus).RequireAuthorization();
    }

    public async Task<Results<Ok<Result>, BadRequest>> UpdateUserMembershipStatus(ISender sender, string applicationUserId, string gymId, [AsParameters] UpdateUserGymMembershipStatusCommand request)
    {
        if (applicationUserId != request.ApplicationUserId || gymId != request.GymId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(request);

        return TypedResults.Ok(result);
    }
}