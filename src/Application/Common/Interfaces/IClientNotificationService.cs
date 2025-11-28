
using FitPass.Application.Common.Models;

namespace FitPass.Application.Common.Interfaces;

public interface IClientNotificationSender
{
    Task Send(string userId, ClientNotification notification);
    Task SendAsync(IEnumerable<string> userIds, ClientNotification notification);
}
