using FitPass.Application.Common.Models;
using FitPass.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public IAsyncEnumerable<ClientNotification> GetClientNotificationStreamerForUser(string userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IClientNotificationStreamer>();

        return service.StreamUserUpdates(userId, CancellationToken.None);
    }
}
