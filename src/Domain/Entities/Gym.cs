namespace FitPass.Domain;

public class Gym : BaseEntity
{
    public required string QRCode { get; set; } //separate from Id => not exposing db id to outside
    public required string Name { get; set; }
    public required string Location { get; set; }
    public string? OwnerName { get; set; }
    public ICollection<GymPassProduct> GymPasses { get; set; } = [];
    public ICollection<UserGymMembership> Memberships { get; set; } = [];
}