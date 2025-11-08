
using FitPass.Application.GymMembershipPassUsages.DTOs;
using FitPass.Application.GymMembershipPassUsages.Queries;
using FitPass.Application.GymPassUsages.Commands;
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
    }

    public async Task<Ok<List<GymPassUsageDto>>> GetGymPassUsagesForMyGymToday(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymPassUsagesForMyGymTodayQuery());

        return TypedResults.Ok(result);
    }

    public async Task<NoContent> GymEmployeeEndUserGymSession(ISender sender, string gymPassUsageId, CancellationToken cancellationToken)
    {
        await sender.Send(new GymEmployeeEndUserGymSessionCommand(gymPassUsageId));

        return TypedResults.NoContent();
    }

    public async Task<NoContent> UpdateGymPassUsageLockerNumberCommand(ISender sender, string gymPassUsageId, [FromBody] string lockerNumber, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateGymPassUsageLockerNumberCommand(gymPassUsageId, lockerNumber));

        return TypedResults.NoContent();
    }
}
