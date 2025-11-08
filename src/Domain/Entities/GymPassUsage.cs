namespace FitPass.Domain.Entities;

public class GymPassUsage : BaseAuditableEntity
{
    public required string ApplicationUserId { get; init; }
    public required string GymId { get; init; }
    //public required PassType PassType { get; init; } - not going to change on GymMembershipPass
    //public required int? TotalPassUses { get; init; } - not going to change on GymMembershipPass
    public required int? RemainingPassUses { get; init; }
    public required DateOnly? PassExpirationDate { get; init; }
    public required PassUseResult PassUseResult { get; init; }
    public required string? LockerNumber { get; set; }
    //Started time can be retrieved from CreatedOn
    public DateTimeOffset? GymSessionFinishedAt {  get; set; }
    public required string PassId { get; init; }
    public GymMembershipPass Pass { get; set; } = null!;

    public GymPassUsage FinishGymSession()
    {
        if (PassUseResult != PassUseResult.Success)
        {
            throw new InvalidOperationException("Cannot end the GymPassUsage if it was not successful.");
        }

        GymSessionFinishedAt = DateTimeOffset.UtcNow;

        return this;
    }

    public TimeSpan? GymSessionLengthToTimeSpan() => GymSessionFinishedAt?.Subtract(CreatedOn);

    public bool HasGymSessionEnded() => GymSessionFinishedAt is not null;
}
