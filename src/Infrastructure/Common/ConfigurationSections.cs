using FitPass.Application.Common.Interfaces;

namespace FitPass.Infrastructure.Common;

public abstract class ConfigurationSections
{
    public const string ClientApp = nameof(ClientApp);
    public const string Jwt = nameof(Jwt);
    public const string Email = nameof(Email);
    public const string Stripe = nameof(Stripe);
    public const string Cultures = nameof(Cultures);
}
