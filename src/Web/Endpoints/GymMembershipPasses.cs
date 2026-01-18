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
        groupBuilder.MapPut(GymEmployeeUseGymMembershipPass, "MyGymMember/Use/{userId}/{gymMembershipPassId}").RequireAuthorization();

        groupBuilder.MapGet(IsGymMembershipPassValid, "MyGymMember/Validity/{gymMembershipPassId}").RequireAuthorization();

        groupBuilder.MapGet(GetMyGymMembershipPasses, "My").RequireAuthorization();
    }

    public async Task<Results<Ok<PassUseResult>, ProblemHttpResult>> GymEmployeeUseGymMembershipPass(ISender sender, string userId, string gymMembershipPassId, [FromBody] string lockerNumber, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GymEmployeeUseGymMembershipPassCommand(gymMembershipPassId, userId, lockerNumber));

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<bool>, ProblemHttpResult>> IsGymMembershipPassValid(ISender sender, string gymMembershipPassId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new IsGymMembershipPassValidQuery(gymMembershipPassId));

        return result.ToTypedResult();
    }

    public async Task<Ok<List<GymMembershipPassDto>>> GetMyGymMembershipPasses(ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyGymMembershipPassesQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }
}
