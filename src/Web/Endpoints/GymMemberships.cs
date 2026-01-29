using FitPass.Application.GymMemberships.Commands;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Application.GymMemberships.Queries;
using FitPass.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class GymMemberships : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UpdateGymMembershipStatus, "/{gymMembershipId}").RequireAuthorization();

        groupBuilder.MapGet(GetGymMembershipsQueryToMyGym, "/MyGym").RequireAuthorization();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> UpdateGymMembershipStatus(
        ISender sender, string gymMembershipId, [FromBody] GymMembershipStatus newGymMembershipStatus)
    {
        var result = await sender.Send(new UpdateGymMembershipStatusCommand(gymMembershipId, newGymMembershipStatus), CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Ok<List<GymMembershipWithUserProfileAndEmailDto>>> GetGymMembershipsQueryToMyGym(ISender sender,  CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymMembershipsToMyGymQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }
}
