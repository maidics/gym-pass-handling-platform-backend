using FitPass.Application.Requests.DTOs;
using FitPass.Application.Requests.Queries;
using FitPass.Application.Requests.Commands;
using FitPass.Application.Requests.Commands.Fulfill;
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

        groupBuilder.MapPut(FulfillOtherTypeRequest, "/Fulfill/Other/Submitted/{requestId}").RequireAuthorization(); //TODO: move fulfill commands to here in same style

        groupBuilder.MapGet(GetMyRequestById, "/My/{requestId}").RequireAuthorization();
    }

    public async Task<Results<Ok<RequestDto>, ProblemHttpResult>> GetRequestById(
        ISender sender, string requestId, CancellationToken cancellationToken)
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
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> CreateGymAdminPromotionRequest(
        ISender sender, [FromBody] CreateGymAdminPromotionRequestCommand command)
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> RejectRequest(
        ISender sender, string requestId, [FromBody] string rationale)
    {
        var result = await sender.Send(new RejectRequestCommand(requestId, rationale), CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Ok<List<RequestDto>>> GetMyRequests(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyRequestsQuery(), cancellationToken);
        
        return TypedResults.Ok(result);
    }

    public async Task<Results<NoContent, ProblemHttpResult>> CancelMyRequest(ISender sender, string requestId)
    {
        var result = await sender.Send(new CancelMyRequestCommand(requestId), CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> CreatePayloadFreeRequest(ISender sender,
        [FromBody] CreatePayloadFreeRequestCommand command)
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> FulfillOtherTypeRequest(ISender sender,
        string requestId)
    {
        var result = await sender.Send(new FulfillOtherTypeRequestCommand(requestId), CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<RequestDto>, ProblemHttpResult>> GetMyRequestById(ISender sender, string requestId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyRequestByIdQuery(requestId), cancellationToken);

        return result.ToTypedResult();
    }
}
