namespace FitPass.Domain.Entities.Payment;

public class ProductPaymentIdentity : BaseAuditableEntity
{
    public required string GymPassProductId { get; set; }
    public required string ProductId { get; set; }
    public required string PriceId { get; set; }
    public ICollection<ArchivedPaymentProviderPrice> ArchivedPaymentProviderPrices { get; set;} = [];
    public GymPassProduct GymPassProduct { get; set; } = null!;
}

public class ArchivedPaymentProviderPrice
{
    public required string Id { get; set; }
    public required DateTimeOffset ArchivedOn { get; set; }
}
