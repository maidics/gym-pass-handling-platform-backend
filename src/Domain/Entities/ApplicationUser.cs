using FitPass.Domain.Constants;

namespace FitPass.Domain.Entities;

public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required ICollection<UserGymMembership>? UserGymMemberships { get; set; }
    public required GymStaffAssigment? GymStaffAssigment { get; set; }
    public ICollection<Request> Requests { get; set; } = [];
    //returns a bool value on wether or not the user is a gym member aka purchased a pass before
    public bool IsGymMember => UserGymMemberships != null && UserGymMemberships.Count > 0;
}
