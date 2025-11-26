
using FitPass.Application.Common.Models;

namespace FitPass.Application.Common.Interfaces;

public interface IClientNotificationSender
{
    Task Send(string userId, ClientNotification notification);
    Task Send(IEnumerable<string> userIds, ClientNotification notification);
}
