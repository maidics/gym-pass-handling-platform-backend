namespace FitPass.Domain.Enums;

public enum PaymentError
{
    CardDeclined,
    InsufficientFunds,
    ExpiredCard,
    InvalidPaymentInfo,
    Unknown
}
