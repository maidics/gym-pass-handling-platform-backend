namespace FitPass.Domain.Entities;

public class GymPassProductTemplate
{
    public required GymTier GymTier { get; set; }
    public required PassType PassType { get; set; }
    public required int? TotalUses { get; set; }
    public required DateTimeOffset? ExpirationDate { get; set; }
    public required decimal EurPrice { get; set; }
}