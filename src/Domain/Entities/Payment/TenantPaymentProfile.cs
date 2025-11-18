namespace FitPass.Domain.Entities.Payment;

public class TenantPaymentProfile : BaseAuditableEntity
{
    public required string GymId { get; set; }
    public string? PaymentTenantAccountId { get; set; }
    public TenantPaymentAccountStatus AccountStatus { get; set; } = TenantPaymentAccountStatus.Default();
}
