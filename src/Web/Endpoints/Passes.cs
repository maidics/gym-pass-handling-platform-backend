
using FitPass.Application;
using FitPass.Application.Common.Models;
using FitPass.Application.Passes.Commands;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class Passes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UsePass, "{id}/Use").RequireAuthorization();

        groupBuilder.MapPut(UserBuyPass, "{id}/Buy").RequireAuthorization();
    }

    public async Task<Results<Ok<Result>, BadRequest>> UsePass(ISender sender, string id, [AsParameters] UsePassCommand command)
    {
        if (id != command.OwnedPassId)
        {
            return TypedResults.BadRequest();
        } 

        var result = await sender.Send(command);

        return TypedResults.Ok(result); 
    }

    public async Task<Results<Ok<Result>, BadRequest>> UserBuyPass(ISender sender, string id, [AsParameters] UserBuyPassCommand command)
    {
        if (id != command.GymPassProductId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }
}
