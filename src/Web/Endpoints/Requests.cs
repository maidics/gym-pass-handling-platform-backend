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

    public async Task<Results<Ok<Result>, ProblemHttpResult>> CreateGymAdminPromotionRequest(ISender sender, [FromBody] CreateGymAdminPromotionRequestCommand command)
    {
        var result = await sender.Send(command);

        if (!result.Succeeded)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Business Rule Violation",
                Detail = result.Errors[0]
            };

            return TypedResults.Problem(problem);
        }

        return TypedResults.Ok(result);
    }

    public async Task<NoContent> RejectRequest(ISender sender, string requestId, CancellationToken cancellationToken)
    {
        await sender.Send(new RejectRequestCommand(requestId));

        return TypedResults.NoContent();
    }
}
