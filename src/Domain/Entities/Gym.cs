namespace FitPass.Domain.Entities;

public class Gym : BaseEntity
{
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required GymStatus Status { get; set; }
    public required GymTier Tier { get; set; }
    public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.UtcNow;
    public string? OwnerName { get; set; }
    public ICollection<GymPassProduct> GymPassProducts { get; set; } = [];
    public ICollection<UserGymMembership> UserGymMemberships { get; set; } = [];
}