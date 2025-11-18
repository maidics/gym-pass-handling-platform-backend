using FitPass.Domain.Events.OwnedPasses;

namespace FitPass.Domain.Entities;

public class GymMembershipPass : BaseAuditableEntity
{
    public required string GymMembershipId { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? RemainingUses { get; set; }
    public required DateOnly? ExpirationDate { get; set; }
    public GymMembership GymMembership { get; set; } = null!;

    public bool IsExpired()
    {
        var utcNow = DateTimeOffset.UtcNow;

        var now = new DateOnly(utcNow.Year, utcNow.Month, utcNow.Day);

        return Type == PassType.Unlimited && ExpirationDate.HasValue && ExpirationDate.Value < now;
    }
    public bool HasNoUsesLeft() => Type != PassType.Unlimited && RemainingUses.HasValue && RemainingUses <= 0;

    public GymPassUsage Use(string? lockerNumber) //GymMembershipMust be loaded
    {
        if (GymMembership is null)
        {
            throw new ArgumentNullException(nameof(GymMembership));
        }

        if (HasNoUsesLeft())
        {
            AddDomainEvent(new PassExpiredEvent(this));

            return new GymPassUsage
            {
                ApplicationUserId = GymMembership.ApplicationUserId,
                GymId = GymMembership.GymId!,
                PassType = Type,
                TotalPassUses = TotalUses,
                RemainingPassUses = RemainingUses,
                PassExpirationDate = ExpirationDate,
                PassUseResult = PassUseResult.AlreadyHasNoUsesLeft,
                PassId = Id,
                LockerNumber = lockerNumber
            };
        }

        if (IsExpired())
        {
            AddDomainEvent(new PassExpiredEvent(this));

            return new GymPassUsage
            {
                ApplicationUserId = GymMembership.ApplicationUserId,
                GymId = GymMembership.GymId!,
                PassType = Type,
                TotalPassUses = TotalUses,
                RemainingPassUses = RemainingUses,
                PassExpirationDate = ExpirationDate,
                PassUseResult = PassUseResult.UnlimitedPassAlreadyExpired,
                PassId = Id,
                LockerNumber = lockerNumber
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
            ApplicationUserId = GymMembership.ApplicationUserId,
            GymId = GymMembership.GymId!,
            PassType = Type,
            TotalPassUses = TotalUses,
            RemainingPassUses = RemainingUses,
            PassExpirationDate = ExpirationDate,
            PassUseResult = PassUseResult.Success,
            PassId = Id,
            LockerNumber = lockerNumber
        };
    }
}
