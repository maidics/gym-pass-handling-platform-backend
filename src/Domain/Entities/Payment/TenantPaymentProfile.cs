namespace FitPass.Domain.Entities.Payment;

public class TenantPaymentProfile : BaseAuditableEntity
{
    public required string GymId { get; set; }
    public required string PaymentAccountId { get; set; }
    public TimeIntervals? PayoutInterval { get; set; }
    public string? PayoutAnchor { get; set; }
    public TenantPaymentAccountStatus AccountStatus { get; set; } = TenantPaymentAccountStatus.Default();
    public DateTimeOffset? LastUpdatedOnPaymentProvidersSide { get; set; }
    public string? LastUpdatedByOnPaymentProvidersSide { get; set; }
    public DateTimeOffset? LastAccountLinkGeneratedOn { get; set; }
    public string? LastAccountLinkGeneratedBy { get; set; } //turn these into collections later?
    public Gym Gym { get; set; } = null!;
}
