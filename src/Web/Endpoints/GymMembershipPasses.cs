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
        groupBuilder.MapGet(GetValidGymMembershipPasses, "My").RequireAuthorization();

        groupBuilder.MapPut(GymEmployeeUseGymMembershipPass, "MyGymMember/Use/{userId}/{gymMembershipPassId}").RequireAuthorization();

        groupBuilder.MapGet(IsGymMembershipPassValid, "MyGymMember/Validity/{gymMembershipPassId}").RequireAuthorization();
    }

    public async Task<Ok<List<GymMembershipPassDto>>> GetValidGymMembershipPasses(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetValidGymMembershipPassesQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<IResult> GymEmployeeUseGymMembershipPass(ISender sender, string userId, string gymMembershipPassId, [FromBody] string lockerNumber, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GymEmployeeUseGymMembershipPassCommand(gymMembershipPassId, userId, lockerNumber));

        return result.ToTypedResult();
    }

    public async Task<IResult> IsGymMembershipPassValid(ISender sender, string gymMembershipPassId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new IsGymMembershipPassValidQuery(gymMembershipPassId));

        return result.ToTypedResult();
    }
}
