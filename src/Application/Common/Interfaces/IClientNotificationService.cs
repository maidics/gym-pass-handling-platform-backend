
using FitPass.Application.Common.Models;

namespace FitPass.Application.Common.Interfaces;

public interface IClientNotificationSender
{
    Task SendAsync(string userId, ClientNotification notification);
    Task SendAsync(IEnumerable<string> userIds, ClientNotification notification);
}
