
using FitPass.Application.Payments.Commands;

namespace FitPass.Web.Endpoints;

public class Payments : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateGymPassProductOnetimePaymentIntent, "/GymPassProducts/OneTime/{gymPassProductId}").RequireAuthorization();
    }

    public async Task<IResult> CreateGymPassProductOnetimePaymentIntent(ISender sender, string gymPassProductId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateGymPassProductOneTimePaymentIntentCommand(gymPassProductId));

        return result.ToTypedResult();
    }
}
