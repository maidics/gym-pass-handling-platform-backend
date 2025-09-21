using Fitpass.Application.Requests.DTOs;
using Fitpass.Application.Requests.Queries;
using FitPass.Application.Common.Models;
using FitPass.Application.Requests.Commands;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fitpass.Web.Endpoints;

public class Requests : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetRequest, "{id}").RequireAuthorization();

        groupBuilder.MapGet(GetRequests).RequireAuthorization();

        groupBuilder.MapPut(UpdateRequestStatus, "{id}/Status").RequireAuthorization();

        groupBuilder.MapPost(CreateGymCreationRequest, "/GymCreation");

        groupBuilder.MapPost(CreateGymAdministratorUserRequest, "GymAdministrator").RequireAuthorization();
    }

    public async Task<Results<Ok<RequestDto>, NotFound, BadRequest>> GetRequest(ISender sender, string id, [AsParameters] GetRequestQuery query)
    {
        if (id != query.RequestId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(query);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<RequestDto>>> GetRequests(ISender sender, [AsParameters] GetRequestsQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<Result>, BadRequest>> UpdateRequestStatus(ISender sender, string id, [AsParameters] UpdateRequestStatusCommand command)
    {
        if (id != command.RequestId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<Result>> CreateGymCreationRequest(ISender sender, [AsParameters] CreateGymCreationRequestCommand command)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<Result>> CreateGymAdministratorUserRequest(ISender sender, [AsParameters] CreateGymAdministratorUserRequestCommand command)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }
}
