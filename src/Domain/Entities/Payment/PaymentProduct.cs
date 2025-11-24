namespace FitPass.Domain.Entities.Payment;

public class ProductPaymentIdentity : BaseEntity
{
    public required string GymPassProductId { get; set; }
    public required string PriceId { get; set; }
    public Dictionary<string, DateTimeOffset> ArchivedPaymentProviderPriceIds { get; set;} = [];
}
