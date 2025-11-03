namespace FitPass.Domain.Entities;

public class PaymentPrice : BaseEntity
{
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public ICollection<PaymentProduct> Products { get; set; } = [];
}
