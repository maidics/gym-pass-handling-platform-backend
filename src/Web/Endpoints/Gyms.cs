
using Fitpass.Application.Gyms.Commands;
using Fitpass.Application.Gyms.DTOs;
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

        groupBuilder.MapGet(GetMyGymQrCode).RequireAuthorization();

        groupBuilder.MapPut(UpdateGymProfile, "{gymId}").RequireAuthorization();
    }

    public async Task<Ok<Result>> RegisterGym(ISender sender, [AsParameters] RegisterGymCommand command)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }

    public async Task<IResult> GetMyGymQrCode(ISender sender, [AsParameters] GetMyGymQrCodeQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.File(result, contentType: "image/png", fileDownloadName: "gymQrCode.png");
    }

    public async Task<Results<Ok<GymDto>, NotFound>> UpdateGymProfile(ISender sender, string gymId, [AsParameters] UpdateGymProfileCommand command)
    {
        var result = await sender.Send(command);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
