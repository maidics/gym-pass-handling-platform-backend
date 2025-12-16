using FitPass.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public static string GetDefaultCulture()
    {
        using var scope = _scopeFactory.CreateScope();
        var localizer = scope.ServiceProvider.GetRequiredService<ILocalizer>();

        return localizer.DefaultCulture;
    }
}
