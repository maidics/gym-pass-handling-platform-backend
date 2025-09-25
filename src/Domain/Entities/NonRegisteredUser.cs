namespace FitPass.Domain.Entities;

public class NonRegisteredUser : BaseAuditableEntity
{
    public required string? Email { get; set; }
    public required string? PhoneNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public ICollection<UserGymMembership> UserGymMemberships { get; set; } = [];
}
