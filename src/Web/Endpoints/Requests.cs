using FitPass.Application.Requests.DTOs;
using FitPass.Application.Requests.Queries;
using FitPass.Application.Common.Models;
using FitPass.Application.Requests.Commands;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class Requests : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetRequest, "{requestId}").RequireAuthorization();

        groupBuilder.MapGet(GetRequests).RequireAuthorization();

        groupBuilder.MapPost(CreateGymCreationRequest, "/GymCreation");

        groupBuilder.MapPost(CreateGymAdminPromotionRequest, "GymAdminNomination").RequireAuthorization();

        groupBuilder.MapPut(RejectRequest, "Reject/{requestId}").RequireAuthorization();
    }

    public async Task<Ok<RequestDto>> GetRequest(ISender sender, string requestId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRequestQuery(requestId), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<RequestDto>>> GetRequests(ISender sender, [FromBody] GetRequestsQuery query, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<NoContent> CreateGymCreationRequest(ISender sender, [FromBody] CreateGymCreationRequestCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> CreateGymAdminPromotionRequest(ISender sender, [FromBody] CreateGymAdminPromotionRequestCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> RejectRequest(ISender sender, string requestId, CancellationToken cancellationToken)
    {
        await sender.Send(new RejectRequestCommand(requestId));

        return TypedResults.NoContent();
    }
}
