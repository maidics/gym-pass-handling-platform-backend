using FitPass.Application.PaymentProviderWebhooks.Commands;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class Webhooks : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(HandleStripeWebhook, "Stripe").AddEndpointFilter<StripeWebHookSignatureFilter>().AllowAnonymous();
    }

    public async Task<NoContent> HandleStripeWebhook(ISender sender, HttpContext httpContext)
    {
        var json = httpContext.Items["StripeWebhookJson"] as string;
        var signature = httpContext.Items["StripeWebhookSignature"] as string;

        await sender.Send(new HandlePaymentProviderWebhookCommand(json!, signature!));

        return TypedResults.NoContent();
    }
}