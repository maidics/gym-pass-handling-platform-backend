
using FitPass.Application;
using FitPass.Application.Common.Models;
using FitPass.Application.Passes.Commands;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class Passes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UsePass, "{id}").RequireAuthorization();

        groupBuilder.MapPut(UserBuyPass, "{id}").RequireAuthorization();
    }

    public async Task<Results<Ok<Result>, BadRequest>> UsePass(ISender sender, string id, [AsParameters] UsePassCommand request)
    {
        if (id != request.passId)
        {
            return TypedResults.BadRequest();
        } 

        var result = await sender.Send(request);

        return TypedResults.Ok(result); 
    }

    public async Task<Results<Ok<Result>, BadRequest>> UserBuyPass(ISender sender, string id, [AsParameters] UserBuyPassCommand request)
    {
        if (id != request.GymPassProductId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(request);

        return TypedResults.Ok(result);
    }
}
