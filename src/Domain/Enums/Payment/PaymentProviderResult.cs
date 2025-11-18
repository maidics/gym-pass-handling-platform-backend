namespace FitPass.Domain.Enums;

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
