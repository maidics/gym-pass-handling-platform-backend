using Microsoft.AspNetCore.Identity;

namespace FitPass.Infrastructure.Identity;
public class ApplicationUser : IdentityUser
{
    public DateTimeOffset CreatedOn {  get; set; } = DateTimeOffset.UtcNow;
}
