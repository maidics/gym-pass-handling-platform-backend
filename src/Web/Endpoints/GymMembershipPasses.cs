using Fitpass.Application.GymMembershipPasses.Queries;
using FitPass.Application.GymMembershipPasses.Commands;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class GymMembershipPasses : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UseGymMembershipPass, "Use/{gymMembershipPassId}").RequireAuthorization();

        groupBuilder.MapGet(GetGymMembershipPassesForGym, "My/{gymId}").RequireAuthorization();
    }

    public async Task<Results<Ok<PassUseResult>, BadRequest<PassUseResult>>> UseGymMembershipPass (ISender sender, string gymMembershipPassId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UseGymMembershipPassCommand(gymMembershipPassId));

        if (result == PassUseResult.AlreadyExpired)
        {
            return TypedResults.BadRequest(result);
        }

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<GymMembershipPassDto>>> GetGymMembershipPassesForGym(ISender sender, [FromQuery] string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymMembershipPassesForGymQuery(gymId), cancellationToken);

        return TypedResults.Ok(result);
    }
}
