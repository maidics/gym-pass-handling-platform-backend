namespace FitPass.Domain.Entities;

public class PurchaseReceipt : BaseEntity
{
    public required string? ApplicationUserId { get; set; }
    public DateTimeOffset PurchaseDateTime { get; set; } = DateTimeOffset.UtcNow;
    public required GymPassProduct GymPassProduct { get; set; }
}