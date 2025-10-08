using FitPass.Domain.Entities;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    public async Task SeedNonRegisteredUsersAsync()
    {
        List<NonRegisteredUser> nonRegisteredUsers = [
                new NonRegisteredUser {
                    Id = "NonRegisteredUser1",
                    Email = "nonregistered@localhost",
                    PhoneNumber = "1234567890",
                    FirstName = "Non",
                    LastName = "Registered",
                    PaymentProfile = new UserPaymentProfile 
                    {
                        ApplicationUserId = null,
                        NonRegisteredUserId = "NonRegisteredUser1"
                    }
                }
            ];

        await _context.NonRegisteredUsers.AddRangeAsync(nonRegisteredUsers);
    }
}
