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
    GymSessionEnded,
    GymStatusUpdatedByAppAdmin,
}

public class ClientNotification
{
    public required string Message { get; init; }
    public required ClientNotificationType Type { get; init; }
}
