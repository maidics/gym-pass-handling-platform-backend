using Stripe;

namespace FitPass.Web.Infrastructure;

public class AnonymousOnlyFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            return TypedResults.Forbid();
        }

        return await next(context);
    }
}

public class StripeWebHookSignatureFilter : IEndpointFilter
{
    private readonly string _webhookSecret;
    private readonly ILogger<StripeWebHookSignatureFilter> _logger;

    public StripeWebHookSignatureFilter(IConfiguration configuration, ILogger<StripeWebHookSignatureFilter> logger)
    {
        _webhookSecret = configuration["Stripe:WebHookSecret"] ?? throw new InvalidOperationException("Stripe webhook secret is not configured");
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var signature = httpContext.Request.Headers["Stripe-Signature"].ToString();

        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogError("Stripe webhook rejected: no signature found in header. HttpContext: {HttpContext} ", httpContext);
        }

        httpContext.Request.EnableBuffering();

        using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
        var json = await reader.ReadToEndAsync();

        httpContext.Request.Body.Position = 0;

        try
        {
            EventUtility.ConstructEvent(json, signature, _webhookSecret);
        } catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook rejected: missing signature header.");
            return Results.Forbid();
        }

        httpContext.Items["StripeWebhookJson"] = json;
        httpContext.Items["StripeWebhookSignature"] = signature;

        return await next(context);
    }
}