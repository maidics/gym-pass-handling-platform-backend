namespace FitPass.Domain.Entities;

public class GymPassUsage : BaseAuditableEntity
{
    public required string ApplicationUserId { get; init; }
    public required string GymId { get; init; }
    public required PassType PassType { get; init; } //this is here so if the pass is archived then still available
    public required int? TotalPassUses { get; init; } //same ^
    public required int? RemainingPassUses { get; init; }
    public required DateOnly? PassExpirationDate { get; init; }
    public required PassUseResult PassUseResult { get; init; }
    public required string? LockerNumber { get; set; }
    //Started time can be retrieved from CreatedOn
    public DateTimeOffset? GymSessionEndedAt {  get; set; }
    public required string PassId { get; init; }
    //public GymMembershipPass Pass { get; set; } = null!;

    public GymPassUsage FinishGymSession()
    {
        if (PassUseResult != PassUseResult.Success)
        {
            throw new InvalidOperationException("Cannot end the GymPassUsage if it was not successful.");
        }

        GymSessionEndedAt = DateTimeOffset.UtcNow;

        return this;
    }

    public TimeSpan? GymSessionLengthToTimeSpan() => GymSessionEndedAt?.Subtract(CreatedOn);

    public bool HasGymSessionEnded() => GymSessionEndedAt is not null;
}
