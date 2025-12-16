
using FitPass.Domain.ValueObjects;

namespace FitPass.Domain.Entities.Payment;

public class PurchaseReceipt : BaseEntity
{
    public required string UserId { get; set; }
    public required string GymId { get; set; }
    public required string GymPassProductId { get; set; }
    public required bool PurchaseSucceeded { get; set; }
    public required DateTimeOffset CreatedOn { get; set; }
    public required Money? Spent { get; set; }
}
