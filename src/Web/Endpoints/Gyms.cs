using Fitpass.Application.ApplicationUsers.Queries;
using Fitpass.Application.Gyms.Commands;
using Fitpass.Application.Gyms.DTOs;
using Fitpass.Application.Gyms.Queries;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Models;
using FitPass.Application.Gyms.Commands;
using FitPass.Application.Gyms.Queries;
using FitPass.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class Gyms : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(RegisterGym, "Register/{requestId}").RequireAuthorization();

        groupBuilder.MapGet(GetMyGymQrCode, "My/QrCode").RequireAuthorization();

        groupBuilder.MapPut(UpdateMyGymProfile, "My/Profile").RequireAuthorization();

        groupBuilder.MapGet(GetAllGyms).RequireAuthorization();

        groupBuilder.MapGet(GetGymDetails, "Details/{gymId}").RequireAuthorization();

        groupBuilder.MapGet(GetNewGymsThisMonth, "NewThisMonth").RequireAuthorization();

        groupBuilder.MapPut(UpdateGymStatus, "{gymId}/Status").RequireAuthorization();

        groupBuilder.MapGet(GetMyGymDetails, "My/Details").RequireAuthorization();
    }

    public async Task<Results<Ok<Result>, BadRequest>> RegisterGym(ISender sender, string requestId, [AsParameters] RegisterGymCommand command)
    {
        if (requestId != command.gymCreationRequestId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }

    public async Task<IResult> GetMyGymQrCode(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyGymQrCodeQuery(), cancellationToken);

        return TypedResults.File(result, contentType: "image/png", fileDownloadName: "gymQrCode.png");
    }

    public async Task<Ok<GymDto>> UpdateMyGymProfile(ISender sender, [FromBody] UpdateMyGymProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<GymDto>>> GetAllGyms(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllGymsQuery { }, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<GymDto>> GetGymDetails(ISender sender, string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymDetailsQuery(gymId), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<GymDto>>> GetNewGymsThisMonth(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNewGymsThisMonthQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<NoContent> UpdateGymStatus(ISender sender, string gymId, [FromBody] GymStatus newGymStatus, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateGymStatusCommand(gymId, newGymStatus), cancellationToken);

        return TypedResults.NoContent();
    }

    public async Task<Ok<GymDto>> GetMyGymDetails(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyGymDetailsQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<NoContent> UpdateMyGymStatus(ISender sender, [FromBody] UpdateMyGymStatusCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent(); 
    }
}
