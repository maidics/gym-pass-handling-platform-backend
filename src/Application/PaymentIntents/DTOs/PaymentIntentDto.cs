namespace FitPass.Application.Payments.DTOs;

public record PaymentIntentDto
{
    public required string ClientSecret { get; set; }
    public required string TenantPaymentAccountId { get; set; } //displays this on for example the Apple Pay sheet
}
