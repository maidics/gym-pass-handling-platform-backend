namespace FitPass.Domain.Entities.Payment;

public class ProductPaymentIdentity : BaseAuditableEntity
{
    public required string GymPassProductId { get; set; }
    public required string PriceId { get; set; }
    public Dictionary<string, DateTimeOffset> ArchivedPaymentProviderPriceIds { get; set;} = [];
    public GymPassProduct GymPassProduct { get; set; } = null!;
}
