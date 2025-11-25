using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;

namespace FitPass.Web.Services;

public interface IClientNotificationStreamer
{
    IAsyncEnumerable<ClientNotification> StreamUserUpdates(string userId, CancellationToken cancellationToken);
}

public class ClientNotificationService : IClientNotificationSender, IClientNotificationStreamer
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, ChannelWriter<ClientNotification>>> _connectedClients = new();
    //one user can have many connections: if multiple devices are connected all of them get the notification

    public async Task SendUserEvent(string userId, ClientNotification notification)
    {
        if (_connectedClients.TryGetValue(userId, out var activeConnections))
        {
            foreach(var writer in activeConnections.Values)
            {
                await writer.WriteAsync(notification);
            }
        }
    }

    public IAsyncEnumerable<ClientNotification> StreamUserUpdates(string userId, CancellationToken cancellationToken)
    {
        var connectionId = Guid.NewGuid();

        var localChannel = Channel.CreateUnbounded<ClientNotification>();

        var userConnections = _connectedClients.GetOrAdd(userId, _ => new ConcurrentDictionary<Guid, ChannelWriter<ClientNotification>>());

        userConnections.TryAdd(connectionId, localChannel.Writer);

        return StreamImpl(userId, connectionId, localChannel.Reader, cancellationToken);
    }

    private async IAsyncEnumerable<ClientNotification> StreamImpl(
        string userId, 
        Guid connectionId, 
        ChannelReader<ClientNotification> reader, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        try
        {
            await foreach(var notification in reader.ReadAllAsync(cancellationToken))
            {
                yield return notification;
            }
        } finally
        {
            if (_connectedClients.TryGetValue(userId, out var userConnections))
            {
                userConnections.TryRemove(connectionId, out _);

                if (userConnections.IsEmpty)
                {
                    _connectedClients.TryRemove(userId, out _);
                }
            }
        }
    }
}
