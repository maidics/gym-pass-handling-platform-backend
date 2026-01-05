using FitPass.Application.Requests.DTOs;
using FitPass.Application.Requests.Queries;
using FitPass.Application.Requests.Commands;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class Requests : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetRequestById, "{requestId}").RequireAuthorization();

        groupBuilder.MapGet(GetRequests).RequireAuthorization();

        groupBuilder.MapPost(CreateGymCreationRequest, "/GymCreation");

        groupBuilder.MapPost(CreateGymAdminPromotionRequest, "GymAdminNomination").RequireAuthorization();

        groupBuilder.MapPut(RejectRequest, "Reject/{requestId}").RequireAuthorization();

        groupBuilder.MapGet(GetMyRequests, "My").RequireAuthorization();

        groupBuilder.MapPut(CancelMyRequest, "My/{requestId}").RequireAuthorization();

        groupBuilder.MapPost(CreatePayloadFreeRequest, "/PayloadFree").RequireAuthorization();
    }

    public async Task<Results<Ok<RequestDto>, ProblemHttpResult>> GetRequestById(ISender sender, string requestId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRequestByIdQuery(requestId), cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<Ok<List<RequestDto>>> GetRequests(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRequestsQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<RequestDto>, ProblemHttpResult>> CreateGymCreationRequest(
        ISender sender, [FromBody] CreateGymCreationRequestCommand command)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<RequestDto>, ProblemHttpResult>> CreateGymAdminPromotionRequest(
        ISender sender, [FromBody] CreateGymAdminPromotionRequestCommand command)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> RejectRequest(ISender sender, string requestId, [FromBody] string rationale, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RejectRequestCommand(requestId, rationale));

        return result.ToTypedResult();
    }

    public async Task<Ok<List<RequestDto>>> GetMyRequests(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyRequestsQuery(), cancellationToken);
        
        return TypedResults.Ok(result);
    }

    public async Task<Results<NoContent, ProblemHttpResult>> CancelMyRequest(ISender sender, string requestId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelMyRequestCommand(requestId));

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<RequestDto>, ProblemHttpResult>> CreatePayloadFreeRequest(ISender sender,
        [FromBody] CreatePayloadFreeRequestCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }
}
