using FitPass.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FitPass.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public ICollection<UserGymMembership> UserGymMemberships { get; set; } = [];
}