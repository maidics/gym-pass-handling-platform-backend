namespace FitPass.Domain.Entities;

public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
{
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public required ICollection<UserGymMembership>? UserGymMemberships { get; set; }
    public required GymStaffAssigment? GymStaffAssigment { get; set; }
}