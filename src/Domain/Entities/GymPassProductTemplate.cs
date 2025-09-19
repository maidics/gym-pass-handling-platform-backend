namespace FitPass.Domain.Entities;

public class GymPassProductTemplate : BaseEntity
{
    public required GymTier GymTier { get; set; }
    public required PassType PassType { get; set; }
    public required int? TotalUses { get; set; }
    public required int? DaysAfterExpiring { get; set; }
    public required decimal EurPrice { get; set; }
}