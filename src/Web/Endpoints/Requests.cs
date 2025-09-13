
using FitPass.Application.Common.Models;
using FitPass.Application.Requests.Commands;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class Requests : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(ChangeRequestStatus, "{id}").RequireAuthorization();

        groupBuilder.MapPost(CreateGymCreationRequest);
    }

    public async Task<Results<Ok<Result>, BadRequest>> ChangeRequestStatus (ISender sender, string id, [AsParameters] ChangeRequestStatusCommand request)
    {
        if (id != request.RequestId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(request);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<Result>> CreateGymCreationRequest (ISender sender, [AsParameters] CreateGymCreationRequestCommand request)
    {
        var result = await sender.Send(request);

        return TypedResults.Ok(result);
    }
}
