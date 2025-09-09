using FitPass.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FitPass.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public ICollection<UserGymMembership> UserGymMemberships { get; set; } = [];
}