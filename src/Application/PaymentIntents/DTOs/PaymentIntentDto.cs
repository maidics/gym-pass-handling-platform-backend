namespace FitPass.Application.PaymentIntents.DTOs;

public record PaymentIntentDto(
    string ClientSecret, 
    string TenantPaymentAccountId); //displays this on for example the Apple Pay sheet
