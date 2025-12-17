using FitPass.Application.Common.Models;
using FitPass.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public static IAsyncEnumerable<ClientNotification> GetClientNotificationStreamerForUser(string userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IClientNotificationStreamer>();

        return service.StreamUserUpdates(userId, CancellationToken.None);
    }

    public static async Task<ClientNotification> ShouldContainNotificationForUser(string userId)
    {
        var enumerator = GetClientNotificationStreamerForUser(userId).GetAsyncEnumerator();
        var hasItem = await enumerator.MoveNextAsync();

        hasItem.ShouldBeTrue();

        var current = enumerator.Current;
        current.ShouldNotBeNull();

        return current;
    }
}
