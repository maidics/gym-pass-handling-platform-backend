namespace FitPass.Domain;

public class OwnedPass : BaseEntity
{
    public required string? UserGymMembershipId { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? RemainingUses { get; set; }
    public required DateTimeOffset? ExpirationDate { get; set; }
    public required double Price { get; set; }
    public required UserGymMembership UserGymMembership { get; set; }

    private bool IsExpired() => Type == PassType.Subscription && ExpirationDate.HasValue && ExpirationDate.Value < DateTimeOffset.UtcNow;
    private bool HasNoUsesLeft() => Type != PassType.Subscription && RemainingUses.HasValue && RemainingUses <= 0;

    public PassUseResult Use()
    {
        if (IsExpired()) //do not want exception thrown: compuationally more expensive, needs a (custom exception +) handler
        {
            return PassUseResult.Expired;
        }

        if (HasNoUsesLeft())
        {
            return PassUseResult.NoUsesLeft;
        }

        if (Type != PassType.Subscription)
        {
            RemainingUses--;
        }

        AddDomainEvent(new PassUsedEvent(this));

        return PassUseResult.Success;
    }
}
/*
    Why not abstract pass entity? 
        - new pass types are less likely to be added
        - querying is more complex => has to be casted again
*/