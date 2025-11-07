
using FitPass.Application.GymMembershipPassUsages.DTOs;
using FitPass.Application.GymMembershipPassUsages.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class GymPassUsages : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetGymPassUsagesForMyGymToday, "MyGym/Today").RequireAuthorization();
    }

    public async Task<Ok<List<GymPassUsageDto>>> GetGymPassUsagesForMyGymToday(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymPassUsagesForMyGymTodayQuery());

        return TypedResults.Ok(result);
    }
}
