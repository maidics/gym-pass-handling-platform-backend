using FitPass.Domain.Events.GymMembershipPasses;

namespace FitPass.Domain.Entities;

public class GymMembershipPass : BaseAuditableEntity
{
    public required string GymMembershipId { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? RemainingUses { get; set; }
    public required DateOnly? ExpirationDate { get; set; }
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
        if (IsExpired() || HasNoUsesLeft())
        {
            AddDomainEvent(new PassExpiredEvent(this));
            
            return PassUseResult.AlreadyExpired;
        }

        if (Type != PassType.Unlimited)
        {
            RemainingUses--;

            if (HasNoUsesLeft())
            {
                AddDomainEvent(new PassExpiredEvent(this));
                return PassUseResult.Expired;
            }
        }

        return PassUseResult.Success;
    }
}
