using Fitpass.Application.OwnedPasses.Queries;
using FitPass.Application;
using FitPass.Application.OwnedPasses.Commands;
using FitPass.Application.OwnedPasses.DTOs;
using FitPass.Application.OwnedPasses.Queries;
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

        groupBuilder.MapPut(UseNonRegisteredUserOwnedPass, "NonRegisteredUsers/{nonRegisteredUserId}/{passId}/Use").RequireAuthorization();

        groupBuilder.MapGet(GetNonRegisteredUserOwnedPassesForCurrentGym, "NonRegisteredUsers/{nonRegisteredUserId}").RequireAuthorization();

        groupBuilder.MapGet(GetUserOwnedPassesForGym, "My/{gymId}").RequireAuthorization();
    }

    public async Task<Ok> ApplicationUserUsePass(ISender sender, string passId, [FromBody] string gymId, CancellationToken cancellationToken)
    {
        await sender.Send(new ApplicationUserUsePassCommand(gymId, passId), cancellationToken);

        return TypedResults.Ok();
    }

    public async Task<Ok> ApplicationUserBuyPass(ISender sender, string gymPassProductId, CancellationToken cancellationToken)
    {
        await sender.Send(new UserBuyPassCommand(gymPassProductId), cancellationToken);

        return TypedResults.Ok();
    }

    public async Task<Ok> UseNonRegisteredUserOwnedPass(ISender sender, string nonRegisteredUserId, string passId, CancellationToken cancellationToken)
    {
        await sender.Send(new UseNonRegisteredUserOwnedPassCommand(nonRegisteredUserId, passId), cancellationToken);

        return TypedResults.Ok();
    }

    public async Task<Ok<List<OwnedPassDto>>> GetNonRegisteredUserOwnedPassesForCurrentGym(ISender sender, string nonRegisteredUserId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNonRegisteredUserOwnedPassesForCurrentGymQuery(nonRegisteredUserId), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<OwnedPassDto>>> GetUserOwnedPassesForGym(ISender sender, string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserOwnedPassesForGymQuery(gymId), cancellationToken);

        return TypedResults.Ok(result);
    }
}
