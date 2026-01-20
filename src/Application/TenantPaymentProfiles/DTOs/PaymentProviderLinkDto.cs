namespace FitPass.Application.TenantPaymentProfiles.DTOs;

public enum PaymentProviderLinkType
{
    LoginLink,
    AccountLink
}

public record PaymentProviderLinkDto(string Url, PaymentProviderLinkType Type, DateTimeOffset? ExpiresAt = null);
