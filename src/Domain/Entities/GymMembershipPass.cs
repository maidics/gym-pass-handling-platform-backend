using FitPass.Domain.Events.GymMembershipPasses;

namespace FitPass.Domain.Entities;

public class GymMembershipPass : BaseAuditableEntity
{
    public required string GymMembershipId { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? RemainingUses { get; set; }
    public required DateOnly? ExpirationDate { get; set; }
    public required decimal HufPrice { get; set; }
    public GymMembership GymMembership { get; set; } = null!;

    private bool IsExpired()
    {
        var utcNow = DateTimeOffset.UtcNow;

        var now = new DateOnly(utcNow.Year, utcNow.Month, utcNow.Day);

        return Type == PassType.Unlimited && ExpirationDate.HasValue && ExpirationDate.Value < now;
    }
    private bool HasNoUsesLeft() => Type != PassType.Unlimited && RemainingUses.HasValue && RemainingUses <= 0;

    public PassUseResult Use()
    {
        if (IsExpired()) //do not want exception thrown: compuationally more expensive, needs a (custom exception +) handler
        {
            AddDomainEvent(new PassExpiredEvent(this));
            return PassUseResult.Expired;
        }

        if (HasNoUsesLeft())
        {
            AddDomainEvent(new PassExpiredEvent(this));
            return PassUseResult.NoUsesLeft;
        }

        if (Type != PassType.Unlimited)
        {
            RemainingUses--;
        }

        return PassUseResult.Success;
    }
}
/*
    Why not abstract pass entity? 
        - new pass types are less likely to be added
        - querying is more complex => has to be casted again
*/
