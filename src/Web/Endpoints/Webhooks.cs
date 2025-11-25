using FitPass.Application.PaymentProviderWebhooks.Commands;

namespace FitPass.Web.Endpoints;

public class Webhooks : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(HandleStripeWebhook, "Stripe").AddEndpointFilter<StripeWebHookSignatureFilter>().AllowAnonymous();
    }

    public async Task<IResult> HandleStripeWebhook(ISender sender, HttpContext httpContext)
    {
        var json = httpContext.Items["StripeWebhookJson"] as string;
        var signature = httpContext.Items["StripeWebhookSignature"] as string;

        var result = await sender.Send(new HandlePaymentProviderWebhookCommand(json!, signature!));

        return result.ToTypedResult();
    }
}