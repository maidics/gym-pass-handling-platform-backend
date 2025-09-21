namespace FitPass.Domain.Entities;

public class GymPassProduct : BaseEntity
{
    public required string GymId { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? DaysAfterExpiring { get; set; }
    public required decimal EurPrice { get; set; }
    public required bool IsAvailable { get; set; }
    public Gym Gym { get; set; } = null!;

    public DateOnly? GetExpirationDate()
    {
        if (DaysAfterExpiring == null)
        {
            return null;
        }

        var utcNow = DateTimeOffset.UtcNow;

        return new DateOnly(utcNow.Year, utcNow.Month, utcNow.Day).AddDays((int)DaysAfterExpiring);
    }
}
