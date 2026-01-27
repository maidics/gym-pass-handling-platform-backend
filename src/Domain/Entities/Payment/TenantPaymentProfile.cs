namespace FitPass.Domain.Entities.Payment;

public class TenantPaymentProfile : BaseAuditableEntity
{
    public required string GymId { get; set; }
    public required string PaymentAccountId { get; set; }
    //public TimeIntervals? PayoutInterval { get; set; }
    //public string? PayoutAnchor { get; set; }
    //public TenantPaymentAccountStatus AccountStatus { get; set; } = TenantPaymentAccountStatus.Default();
    public DateTimeOffset? LastAccountLinkGeneratedOn { get; set; }
    public string? LastAccountLinkGeneratedBy { get; set; }
    public Gym Gym { get; set; } = null!;
}
