using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private async Task SeedGymsAsync()
    {
        var now = DateTimeOffset.UtcNow;

        var gyms = new List<Gym>()
        {
            new Gym
            {
                Id = "DemoGymId",
                Name = "The Local Pump",
                Address = new Address("Szent István király út 1", null, "Budapest", null, "1111", "HU"),
                Status = GymStatus.Active,
                Tier = GymTier.Elite,
                CreatedOn = now,
                CreatedBy = "GymAdminLocalhostId",
                LastModifiedOn = now,
                LastModifiedBy = null,
                ContactInfos =
                [
                    new GymContactInfo
                    {
                        FullName = "Tóth Elemér",
                        Email = "toth.elemer@localpump.hu",
                        PhoneNumber = PhoneNumber.Create("+36301234567"),
                        Address = new Address(
                            "Váci út 12",
                            "Building B, Suite 400",
                            "Budapest",
                            "Pest",
                            "1134",
                            "HU"
                        ),
                    },
                    new GymContactInfo
                    {
                        FullName = "Kovács Sára",
                        Email = "operations@localpump.hu",
                        PhoneNumber = PhoneNumber.Create("+36709876543"),
                        Address = new Address(
                            "Kossuth Lajos utca 5",
                            null,
                            "Debrecen",
                            "Hajdú-Bihar",
                            "4024",
                            "HU"
                        ),
                    },
                    new GymContactInfo
                    {
                        FullName = "Számlázási Osztály",
                        Email = "invoices@localpump.hu",
                        PhoneNumber = PhoneNumber.Create("+3612345678"),
                        Address = new Address(
                            "Andrássy út 22",
                            "Floor 2",
                            "Budapest",
                            "Pest",
                            "1061",
                            "HU"
                        ),
                    },
                ],
                PaymentProfile = new TenantPaymentProfile()
                {
                    PaymentAccountId = "acct_1SsMj4PEBQsxcoAF", //replace this
                    GymId = "DemoGymId",
                    CreatedOn = DateTimeOffset.UtcNow,
                },
            }
        };

        await _context.Gyms.AddRangeAsync(gyms);
        await _context.SaveChangesAsync();
    }
}
