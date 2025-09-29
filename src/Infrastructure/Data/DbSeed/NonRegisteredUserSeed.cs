using FitPass.Domain.Entities;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    public async Task SeedNonRegisteredUsersAsync()
    {
        List<NonRegisteredUser> nonRegisteredUsers = [
                new NonRegisteredUser {
                    Email = "nonregistered@localhost",
                    PhoneNumber = "1234567890",
                    FirstName = "Non",
                    LastName = "Registered"
                }
            ];

        await _context.NonRegisteredUsers.AddRangeAsync(nonRegisteredUsers);
    }
}
