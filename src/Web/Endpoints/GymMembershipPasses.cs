using FitPass.Application.GymMembershipPasses.Queries;
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

        groupBuilder.MapPut(GymEmployeeUseGymMembershipPass, "MyGymMember/Use/{gymMembershipPassId}").RequireAuthorization();
    }

    public async Task<Results<Ok<PassUseResult>, BadRequest<PassUseResult>>> UseGymMembershipPass (ISender sender, string gymMembershipPassId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UseGymMembershipPassCommand(gymMembershipPassId));

        if (result == PassUseResult.AlreadyHasNoUsesLeft)
        {
            return TypedResults.BadRequest(result);
        }

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<GymMembershipPassDto>>> GetGymMembershipPassesForGym(ISender sender, string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymMembershipPassesForGymQuery(gymId), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<PassUseResult>> GymEmployeeUseGymMembershipPass(ISender sender, string gymMembershipPassId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GymEmployeeUseGymMembershipPassCommand(gymMembershipPassId));

        return TypedResults.Ok(result);
    }
}
