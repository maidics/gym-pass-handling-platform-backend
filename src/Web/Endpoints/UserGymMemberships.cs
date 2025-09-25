using Fitpass.Application.UserGymMemberships.Commands;
using FitPass.Application.Common.Models;
using FitPass.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fitpass.Web.Endpoints;

public class UserGymMemberships : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UpdateUserMembershipStatus, "/{userGymMembership}").RequireAuthorization();
    }

    public async Task<Ok> UpdateUserMembershipStatus(ISender sender, string userGymMembership, [FromBody] GymMembershipStatus newGymMembershipStatus, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateUserGymMembershipStatusCommand(userGymMembership, newGymMembershipStatus), cancellationToken);

        return TypedResults.Ok();
    }
}
