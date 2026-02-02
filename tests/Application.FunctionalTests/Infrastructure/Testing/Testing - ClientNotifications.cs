using FitPass.Application.Common.Models;
using FitPass.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.Infrastructure.Testing;

public partial class Testing
{
    public static IAsyncEnumerable<ClientNotification> GetClientNotificationStreamerForUser(
        string userId
    )
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IClientNotificationStreamer>();

        return service.StreamUserUpdates(userId, CancellationToken.None);
    }

    public static Task<ClientNotification> ShouldContainNotificationForUserAsync(string userId)
    {
        return Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ClientNotificationService>();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                await foreach (var notification in service.StreamUserUpdates(userId, cts.Token))
                {
                    return notification;
                }
            }
            catch (OperationCanceledException) { }

            throw new ShouldAssertException("Stream ended without any data.");
        });
    }
}
