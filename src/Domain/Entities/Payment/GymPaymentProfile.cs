using FitPass.Domain.ValueObjects;

namespace FitPass.Domain.Entities.Payment;

public class GymPaymentProfile : BaseAuditableEntity
{
    public required string GymId { get; set; }
    public string? PaymentTenantAccountId { get; set; }

    public required bool PaymentSetupCompleted { get; set; }
    public required bool ChargesEnabledPayoutsEnabled { get; set; }
    public required bool PayoutsEnabled { get; set; }

    public required string BusinessName { get; set; }
    public required string TaxId { get; set; }
    public required Address BusinessAddress { get; set; }
    public BusinessRepresentative? Representative { get; set; }
    
    public string? BankAccountLast4 { get; set; }
    public string? BankAccountHolderName { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountCurrency { get; set; } 
}
