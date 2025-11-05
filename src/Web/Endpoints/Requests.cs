using Fitpass.Application.Gyms.DTOs;
using Fitpass.Application.Requests.DTOs;
using Fitpass.Application.Requests.Queries;
using FitPass.Application.Common.Models;
using FitPass.Application.Requests.Commands;
using FitPass.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fitpass.Web.Endpoints;

public class Requests : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetRequest, "{requestId}").RequireAuthorization();

        groupBuilder.MapGet(GetRequests).RequireAuthorization();

        groupBuilder.MapPut(UpdateRequestStatus, "{requestId}/Status").RequireAuthorization();

        groupBuilder.MapPost(CreateGymCreationRequest, "/GymCreation");

        groupBuilder.MapPost(CreateGymAdminPromotionRequest, "GymAdminNomination").RequireAuthorization();

        groupBuilder.MapPut(HandleGymCreationRequest, "GymCreation/{requestId}").RequireAuthorization();
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

    public async Task<NoContent> UpdateRequestStatus(ISender sender, string requestId, [FromBody] RequestStatus newRequestStatus, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateRequestStatusCommand(requestId, newRequestStatus), cancellationToken);

        return TypedResults.NoContent();
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

    public async Task<Ok<GymDto>> HandleGymCreationRequest(ISender sender, [FromQuery] string requestId, [FromBody] RequestStatus newStatus, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new HandleGymCreationRequestCommand(requestId, newStatus));

        return TypedResults.Ok(result);
    }
}
