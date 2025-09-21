using Fitpass.Application.Gyms.Commands;
using Fitpass.Application.Gyms.DTOs;
using Fitpass.Application.Gyms.Queries;
using FitPass.Application.Common.Models;
using FitPass.Application.Gyms.Commands;
using FitPass.Application.Gyms.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class Gyms : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(RegisterGym).RequireAuthorization();

        groupBuilder.MapGet(GetMyGymQrCode, "/QrCode").RequireAuthorization();

        groupBuilder.MapPut(UpdateGymProfile, "{gymId}").RequireAuthorization();

        groupBuilder.MapGet(GetAllGyms).RequireAuthorization();

        groupBuilder.MapGet(GetGymDetails, "{id}").RequireAuthorization();

        groupBuilder.MapGet(GetNewGymsThisMonth, "New").RequireAuthorization();

        groupBuilder.MapPut(UpdateGymStatus, "{id}").RequireAuthorization();
    }

    public async Task<Ok<Result>> RegisterGym(ISender sender, [AsParameters] RegisterGymCommand command)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }

    public async Task<IResult> GetMyGymQrCode(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyGymQrCodeQuery { }, cancellationToken);

        return TypedResults.File(result, contentType: "image/png", fileDownloadName: "gymQrCode.png");
    }

    public async Task<Results<Ok<GymDto>, NotFound>> UpdateGymProfile(ISender sender, string gymId, [AsParameters] UpdateGymProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<GymDto>>> GetAllGyms(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllGymsQuery { }, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<GymDto>, NotFound, BadRequest>> GetGymDetails(ISender sender, string id, [AsParameters] GetGymDetailsQuery query, CancellationToken cancellationToken)
    {
        if (id != query.GymId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(query, cancellationToken);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<GymDto>>> GetNewGymsThisMonth(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNewGymsThisMonthQuery { }, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<Result>, BadRequest>> UpdateGymStatus(ISender sender, string id, UpdateGymStatusCommand command, CancellationToken cancellationToken)
    {
        if (id != command.GymID)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}
