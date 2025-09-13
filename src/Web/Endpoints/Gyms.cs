
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
    }

    public async Task<Ok<Result>> RegisterGym (ISender sender, [AsParameters] RegisterGymCommand request)
    {
        var result = await sender.Send(request);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<byte[]>> GetMyGymQrCode (ISender sender, [AsParameters] GetMyGymQrCodeQuery request)
    {
        var result = await sender.Send(request);

        return TypedResults.Ok(result);
    }
}
