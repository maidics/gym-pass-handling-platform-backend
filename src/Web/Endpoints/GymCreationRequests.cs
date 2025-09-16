
using Fitpass.Application.GymCreationRequests.Queries;
using FitPass.Application.Common.Models;
using FitPass.Application.Requests.Commands;
using FitPass.Application.Requests.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class GymCreationRequests : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateGymCreationRequest);

        groupBuilder.MapGet(GetAllGymCreationRequests).RequireAuthorization();

        groupBuilder.MapGet(GetGymCreationRequest).RequireAuthorization();
    }

    public async Task<Ok<Result>> CreateGymCreationRequest(ISender sender, [AsParameters] CreateGymCreationRequestCommand request)
    {
        var result = await sender.Send(request);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<GymCreationRequestDto>>> GetAllGymCreationRequests(ISender sender, [AsParameters] GetAllGymCreationRequestsQuery request)
    {
        var result = await sender.Send(request);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<GymCreationRequestDto>, NotFound>> GetGymCreationRequest(ISender sender, [AsParameters] GetGymCreationRequestQuery request)
    {
        var result = await sender.Send(request);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
