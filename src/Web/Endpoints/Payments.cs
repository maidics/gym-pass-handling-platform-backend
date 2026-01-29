
using FitPass.Application.GymPassProducts.Commands;
using FitPass.Application.PaymentIntents.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class Payments : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateGymPassProductOnetimePaymentIntent, "/GymPassProducts/OneTime/{gymPassProductId}").RequireAuthorization();
    }

    public async Task<Results<Ok<PaymentIntentDto>, ProblemHttpResult>> CreateGymPassProductOnetimePaymentIntent(
        ISender sender, string gymPassProductId)
    {
        var result = await sender.Send(new CreateGymPassProductOneTimePaymentIntentCommand(gymPassProductId), CancellationToken.None);

        return result.ToTypedResult();
    }
}
