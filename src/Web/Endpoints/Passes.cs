
using FitPass.Application;
using FitPass.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web;

public class Passes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UsePass, "{id}").RequireAuthorization(); //
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
}