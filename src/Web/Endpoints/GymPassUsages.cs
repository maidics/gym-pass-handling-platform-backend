using FitPass.Application.GymPassUsages.Commands;
using FitPass.Application.GymPassUsages.DTOs;
using FitPass.Application.GymPassUsages.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class GymPassUsages : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetGymPassUsagesForMyGymToday, "MyGym/Today").RequireAuthorization();

        groupBuilder.MapPut(GymEmployeeEndUserGymSession, "MyGym/EndGymSession/{gymPassUsageId}").RequireAuthorization();

        groupBuilder.MapPut(UpdateGymPassUsageLockerNumberCommand, "MyGym/UpdateLockerNumber/{gymPassUsageId}").RequireAuthorization();

        groupBuilder.MapGet(GetMyGymPassUsages, "My").RequireAuthorization();
    }

    public async Task<Ok<List<GymPassUsageDto>>> GetGymPassUsagesForMyGymToday(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymPassUsagesForMyGymTodayQuery());

        return TypedResults.Ok(result);
    }

    public async Task<Results<NoContent, ProblemHttpResult>> GymEmployeeEndUserGymSession(ISender sender, string gymPassUsageId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new EndUserGymSessionCommand(gymPassUsageId));

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> UpdateGymPassUsageLockerNumberCommand(ISender sender, string gymPassUsageId, [FromBody] string lockerNumber, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateGymPassUsageLockerNumberCommand(gymPassUsageId, lockerNumber));

        return result.ToTypedResult();
    }

    public async Task<Ok<List<GymPassUsageDto>>> GetMyGymPassUsages(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyGymPassUsagesQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }
}
