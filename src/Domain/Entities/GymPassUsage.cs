namespace FitPass.Domain.Entities;

public class GymPassUsage : BaseAuditableEntity
{
    public required string ApplicationUserId { get; init; }
    public required string GymId { get; init; }
    public required PassType PassType { get; init; }
    public required int? TotalPassUses { get; init; }
    public required int? RemainingPassUses { get; init; }
    public required DateOnly? PassExpirationDate { get; init; }
    public required PassUseResult Result { get; init; }
    public required string? LockerNumber { get; set; }
    //Started time can be retrieved from CreatedOn
    public DateTimeOffset? GymSessionFinishedAt {  get; set; }
    public required string GymMembershipPassId { get; init; }
    public GymMembershipPass Pass { get; set; } = null!;

    public GymPassUsage FinishGymSession()
    {
        if (Result != PassUseResult.Success)
        {
            throw new InvalidOperationException("Cannot end the GymPassUsage if it was not successful.");
        }

        GymSessionFinishedAt = DateTimeOffset.UtcNow;

        return this;
    }

    //TODO: add calculation if the result is success and it ended then how long was the gym session

    public bool HasGymSessionEnded() => GymSessionFinishedAt is not null;
}
