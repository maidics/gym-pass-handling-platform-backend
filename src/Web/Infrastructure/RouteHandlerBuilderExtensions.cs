namespace FitPass.Web.Infrastructure;

public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder AllowAnonymousOnly(this RouteHandlerBuilder  builder)
    {
        builder.AddEndpointFilter<AnonymousOnlyFilter>();

        return builder;
    }
}