namespace FitPass.Domain.Entities;

public class GymPassProduct : BaseEntity
{
    public required string GymId { get; set; }
    public required PassType PassType { get; set; }
    public required int? TotalUses { get; set; }
    public required DateTimeOffset? ExpirationDate { get; set; }
    public required decimal EurPrice { get; set; }
    public required bool IsAvailable { get; set; }
    public Gym Gym { get; set; } = null!;
}