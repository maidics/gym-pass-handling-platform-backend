namespace Fitpass.Web.Infrastructure;

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