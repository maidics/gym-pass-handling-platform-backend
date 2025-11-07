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

    public GymPassUsage Use(string userId) //passing this instead of using GymMembership because that might not be loaded => exception
    {
        if (HasNoUsesLeft())
        {
            AddDomainEvent(new PassExpiredEvent(this));

            return new GymPassUsage
            {
                ApplicationUserId = userId,
                PassType = Type,
                TotalPassUses = TotalUses,
                RemainingPassUses = RemainingUses,
                PassExpirationDate = ExpirationDate,
                Result = PassUseResult.AlreadyHasNoUsesLeft,
                GymMembershipPassId = GymMembershipId
            };
        }

        if (IsExpired())
        {
            AddDomainEvent(new PassExpiredEvent(this));

            return new GymPassUsage
            {
                ApplicationUserId = userId,
                PassType = Type,
                TotalPassUses = TotalUses,
                RemainingPassUses = RemainingUses,
                PassExpirationDate = ExpirationDate,
                Result = PassUseResult.Expired,
                GymMembershipPassId = GymMembershipId
            };
        }

        if (Type != PassType.Unlimited)
        {
            RemainingUses--;

            if (HasNoUsesLeft())
            {
                AddDomainEvent(new PassExpiredEvent(this));
            }
        }

        return new GymPassUsage
        {
            ApplicationUserId = userId,
            PassType = Type,
            TotalPassUses = TotalUses,
            RemainingPassUses = RemainingUses,
            PassExpirationDate = ExpirationDate,
            Result = PassUseResult.Success,
            GymMembershipPassId = GymMembershipId
        };
    }
}
