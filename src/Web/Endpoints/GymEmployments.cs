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

        groupBuilder.MapGet(GetGymEmploymentsByGymId, "Gyms/{gymId}").RequireAuthorization();

        groupBuilder.MapGet(GetGymEmploymentById, "{gymEmploymentId}").RequireAuthorization();
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

    public async Task<Results<Ok<List<GymEmploymentDto>>, ProblemHttpResult>> GetGymEmploymentsByGymId(ISender sender, string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymEmploymentsByGymIdQuery(gymId), cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<GymEmploymentDto>, ProblemHttpResult>> GetGymEmploymentById(ISender sender,
        string gymEmploymentId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymEmploymentByIdQuery(gymEmploymentId), cancellationToken);

        return result.ToTypedResult();
    }
}
