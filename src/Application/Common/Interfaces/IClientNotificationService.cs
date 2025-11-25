
using FitPass.Application.Common.Models;

namespace FitPass.Application.Common.Interfaces;

public interface IClientNotificationSender
{
    Task SendUserEvent(string userId, ClientNotification notification);
}
