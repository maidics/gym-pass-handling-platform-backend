namespace FitPass.Domain.Enums.Payment;

public enum PaymentProviderResult
{
    Success,
    ConnectedAccountNotFound,
    Unexpected,
    TooManyRequests,
    PaymentRequired,
    InvalidRequest,
    FailedToCreateConnectedAccount,
    FailedToGenerateAccountLink
}
