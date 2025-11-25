using FitPass.Application.GymEmployments.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using FitPass.Application.GymEmployments.Queries;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class GymEmployments : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetMyGymEmployments, "/MyGym/All").RequireAuthorization();

        groupBuilder.MapGet(GetMyGymEmployment, "My").RequireAuthorization();

        groupBuilder.MapGet(GetGymEmploymentsByGymId, "Gym/{gymId}").RequireAuthorization();
    }

    public async Task<Ok<List<GymEmploymentDto>>> GetMyGymEmployments(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyGymEmploymentsQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<GymEmploymentDto>> GetMyGymEmployment(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyGymEmploymentQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<IResult> GetGymEmploymentsByGymId(ISender sender, [FromQuery] string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymEmploymentsByGymIdQuery(gymId), cancellationToken);

        return result.ToTypedResult();
    }
}
