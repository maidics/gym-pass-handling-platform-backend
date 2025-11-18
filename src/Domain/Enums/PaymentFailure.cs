namespace FitPass.Domain.Enums;

public enum PaymentFailure
{
    ConnectedAccountNotFound,
    Unexpected,
    TooManyRequests,
    PaymentRequired,
    InternalServerError
}
