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
        groupBuilder.MapPut(UpdateUserMembershipStatus, "/{gymMembership}").RequireAuthorization();

        groupBuilder.MapGet(GetGymMembershipsQueryToMyGym, "/MyGym").RequireAuthorization();
    }

    public async Task<NoContent> UpdateUserMembershipStatus(ISender sender, string gymMembership, [FromBody] GymMembershipStatus newGymMembershipStatus, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateGymMembershipStatusCommand(gymMembership, newGymMembershipStatus), cancellationToken);

        return TypedResults.NoContent();
    }

    public async Task<Ok<List<GymMembershipWithUserProfileAndEmailDto>>> GetGymMembershipsQueryToMyGym(ISender sender, [FromBody] GetGymMembershipsQueryToMyGymQuery query, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
