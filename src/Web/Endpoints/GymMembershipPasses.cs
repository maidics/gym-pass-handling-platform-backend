using FitPass.Application.GymMembershipPasses.Queries;
using FitPass.Application.GymMembershipPasses.Commands;
using FitPass.Application.GymMembershipPasses.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class GymMembershipPasses : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UseGymMembershipPass, "Use/{gymMembershipPassId}").RequireAuthorization();

        groupBuilder.MapPost(UserBuyGymMembershipPass, "Buy/Gym/{gymPassProductId}").RequireAuthorization();

        groupBuilder.MapGet(GetGymMembershipPassesForGym, "My/{gymId}").RequireAuthorization();
    }

    public async Task<NoContent> UseGymMembershipPass (ISender sender, string gymMembershipPassId, CancellationToken cancellationToken)
    {
        await sender.Send(new UseGymMembershipPassCommand(gymMembershipPassId));

        return TypedResults.NoContent();
    }

    public async Task<Ok<GymMembershipPassDto>> UserBuyGymMembershipPass(ISender sender, [FromQuery] string gymPassProductId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UserBuyGymMembershipPassCommand(gymPassProductId));

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<GymMembershipPassDto>>> GetGymMembershipPassesForGym(ISender sender, [FromQuery] string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymMembershipPassesForGymQuery(gymId), cancellationToken);

        return TypedResults.Ok(result);
    }
}
