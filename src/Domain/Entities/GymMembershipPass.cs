
using FitPass.Domain.Events.GymMembershipPasses;

namespace FitPass.Domain.Entities;

//Factory method on GymPassProduct
public class GymMembershipPass : BaseAuditableEntity
{
    public required string GymMembershipId { get; set; }
    public required string UserId { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? RemainingUses { get; set; }
    public required DateTimeOffset? ExpirationDate { get; set; }
    public GymMembership GymMembership { get; set; } = null!;

    public bool IsValid(DateTimeOffset utcNow)
    {
        if (RemainingUses is not null)
        {
            return RemainingUses > 0;
        }

        if (ExpirationDate is not null)
        {
            return ExpirationDate.Value.UtcDateTime.Date >= utcNow.UtcDateTime.Date;
        }

        throw new ArgumentException($"Both {nameof(RemainingUses)} and {nameof(ExpirationDate)} is null.");
    }

    public GymPassUsage Use(string gymId, string lockerNumber, DateTimeOffset utcNow) //GymMembership must be loaded
    {
        if (!IsValid(utcNow))
        {
            AddDomainEvent(new PassExpiredEvent(this)); //not throwing here because then Domain event can archive this - this should never happen

            return new GymPassUsage
            {
                UserId = UserId,
                GymId = gymId,
                PassType = Type,
                TotalPassUses = TotalUses,
                RemainingPassUses = RemainingUses,
                PassExpirationDate = ExpirationDate,
                PassUseResult = PassUseResult.Expired,
                PassId = Id,
                LockerNumber = lockerNumber
            };
        }

        if (Type != PassType.Unlimited)
        {
            RemainingUses--;

            if (!IsValid(utcNow))
            {
                AddDomainEvent(new PassExpiredEvent(this));
            }
        }

        return new GymPassUsage
        {
            UserId = UserId,
            GymId = gymId,
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
