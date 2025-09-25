using FitPass.Application;
using FitPass.Application.Passes.Commands;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class OwnedPasses : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(ApplicationUserUsePass, "{passId}/Use").RequireAuthorization();

        groupBuilder.MapPut(ApplicationUserBuyPass, "Buy/{gymPassProductId}").RequireAuthorization();
    }

    public async Task<Ok> ApplicationUserUsePass(ISender sender, string passId, [FromBody] string gymId, CancellationToken cancellationToken)
    {
        await sender.Send(new ApplicationUserUsePassCommand(gymId, passId), cancellationToken);

        return TypedResults.Ok(); 
    }

    public async Task<Ok> ApplicationUserBuyPass(ISender sender, string gymPassProductId, CancellationToken cancellationToken)
    {
        await sender.Send(new ApplicationUserBuyPassCommand(gymPassProductId), cancellationToken);

        return TypedResults.Ok();
    }
}
