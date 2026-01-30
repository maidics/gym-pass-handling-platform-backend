namespace FitPass.Application.Common.Models;

public enum ClientNotificationType
{
    Default,
    Error,
    PaymentFailed,
    GymMembershipStatusChange,
    GymPassProductPurchaseFulfillmentFailed,
    SuccessfulPurchase,
    GymPassUsageLockerUpdated,
    GymSessionEnded
}

public class ClientNotification
{
    public string Message { get;}
    public ClientNotificationType Type { get; }
    public object? Payload { get; }

    private ClientNotification(string message, ClientNotificationType type, object? payload)
    {
        Message = message;
        Type = type;
        Payload = payload;
    }

    public static ClientNotification Create(string message, ClientNotificationType type)
    {
        return new ClientNotification(message, type, null);
    }

    public static ClientNotification Create<T>(string message, ClientNotificationType type, T payload)
    {
        return new ClientNotification(message, type, payload);
    }
}
