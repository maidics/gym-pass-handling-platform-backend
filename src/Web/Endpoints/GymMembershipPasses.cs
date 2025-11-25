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
        groupBuilder.MapGet(GetGymMembershipPassesForGym, "My/{gymId}").RequireAuthorization();

        groupBuilder.MapPut(GymEmployeeUseGymMembershipPass, "MyGymMember/Use/{gymMembershipPassId}").RequireAuthorization();

        groupBuilder.MapGet(IsGymMembershipPassValid, "MyGymMember/Validity/{gymMembershipPassId}").RequireAuthorization();
    }

    public async Task<Ok<List<GymMembershipPassDto>>> GetGymMembershipPassesForGym(ISender sender, string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymMembershipPassesForGymQuery(gymId), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<IResult> GymEmployeeUseGymMembershipPass(ISender sender, string gymMembershipPassId, [FromBody] string lockerNumber, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GymEmployeeUseGymMembershipPassCommand(gymMembershipPassId, lockerNumber));

        return result.ToTypedResult();
    }

    public async Task<Ok<bool>> IsGymMembershipPassValid(ISender sender, string gymMembershipPassId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new IsGymMembershipPassValidQuery(gymMembershipPassId));

        return TypedResults.Ok(result);
    }
}
