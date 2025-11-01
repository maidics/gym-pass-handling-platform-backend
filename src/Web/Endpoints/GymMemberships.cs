using FitPass.Application.GymMemberships.Commands;
using FitPass.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fitpass.Web.Endpoints;

public class GymMemberships : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UpdateUserMembershipStatus, "/{gymMembership}").RequireAuthorization();
    }

    public async Task<Ok> UpdateUserMembershipStatus(ISender sender, string gymMembership, [FromBody] GymMembershipStatus newGymMembershipStatus, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateUserGymMembershipStatusCommand(gymMembership, newGymMembershipStatus), cancellationToken);

        return TypedResults.Ok();
    }

    public async Task<NoContent> AddUserGymMembershipToUser(ISender sender, [FromBody] AddUserGymMembershipToUserCommand command,  CancellationToken cancellationToken)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }
}
