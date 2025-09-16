
using FitPass.Application.Common.Models;
using FitPass.Application.Requests.Commands;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fitpass.Web.Endpoints;

public class Requests : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UpdateRequestStatus, "{id}").RequireAuthorization();
    }

    public async Task<Results<Ok<Result>, BadRequest>> UpdateRequestStatus (ISender sender, string id, [AsParameters] UpdateRequestStatusCommand request)
    {
        if (id != request.RequestId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(request);

        return TypedResults.Ok(result);
    }
}