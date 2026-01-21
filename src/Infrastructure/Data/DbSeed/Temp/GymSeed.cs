using FitPass.Domain.Entities;
using FitPass.Domain.Entities.ContactInfos;
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
                Id = "TestGymId",
                Name = "Test Gym",
                Address = new Address("100 Broadway", "Suite 500", "New York", "NY", "10005", "US"),
                Status = GymStatus.Active,
                Tier = GymTier.Elite,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null,
                ContactInfos = [
                    new GymContactInfo
                    {
                        FullName = "Alexander Thorne", 
                        Email = "alex.thorne@ironfitness.hu",
                        PhoneNumber = PhoneNumber.Create("+36301234567"),
                        Address = new Address(
                            "Váci út 12", 
                            "Building B, Suite 400", 
                            "Budapest", 
                            "Pest", 
                            "1134", 
                            "HU"
                        )
                    },
                    new GymContactInfo
                    {
                        FullName = "Sarah Kovacs",
                        Email = "operations@metroflex.com",
                        PhoneNumber = PhoneNumber.Create("+36709876543"),
                        Address = new Address(
                            "Kossuth Lajos utca 5", 
                            null,
                            "Debrecen", 
                            "Hajdú-Bihar", 
                            "4024", 
                            "HU"
                        )
                    },
                    new GymContactInfo
                    {
                        FullName = "Billing Department",
                        Email = "invoices@gymnetwork.eu",
                        PhoneNumber = PhoneNumber.Create("+3612345678"),
                        Address = new Address(
                            "Andrássy út 22", 
                            "Floor 2", 
                            "Budapest", 
                            "Pest", 
                            "1061", 
                            "HU"
                        )
                    }
                ],
                /*
                PaymentProfile = new TenantPaymentProfile()
                {
                    PaymentAccountId = "acct_1SrIJtPSsrSaufPx",
                    GymId = "TestGymId"
                }
                */
            },

            // 2. Local Gym in London
            new Gym
            {
                Name = "The Local Pump",
                Address = new Address("15 Baker Street", null, "London", null, "NW1 6XE", "GB"),
                Status = GymStatus.Active,
                Tier = GymTier.Local,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 3. MidRange in Toronto
            new Gym
            {
                Name = "Maple Leaf Fitness",
                Address = new Address("450 Yonge St", null, "Toronto", "ON", "M4Y 1W9", "CA"),
                Status = GymStatus.Active,
                Tier = GymTier.MidRange,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 4. Premium in Berlin
            new Gym
            {
                Name = "Kraftwerk Berlin",
                Address = new Address("Alexanderplatz 1", null, "Berlin", null, "10178", "DE"),
                Status = GymStatus.Active,
                Tier = GymTier.Premium,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 5. Suspended Local Gym
            new Gym
            {
                Name = "Basement Barbell",
                Address = new Address("88 Industrial Way", null, "Chicago", "IL", "60601", "US"),
                Status = GymStatus.Suspended,
                Tier = GymTier.Local,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 6. Active MidRange in Paris
            new Gym
            {
                Name = "Le Gym Parisien",
                Address = new Address("12 Rue de Rivoli", null, "Paris", null, "75001", "FR"),
                Status = GymStatus.Active,
                Tier = GymTier.MidRange,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 7. Active Elite in Los Angeles
            new Gym
            {
                Name = "Gold Coast Athletics",
                Address = new Address("500 Santa Monica Blvd", null, "Santa Monica", "CA", "90401", "US"),
                Status = GymStatus.Active,
                Tier = GymTier.Elite,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 8. Inactive Local Gym
            new Gym
            {
                Name = "Old School Iron",
                Address = new Address("22 Rust Bucket Rd", null, "Detroit", "MI", "48201", "US"),
                Status = GymStatus.Inactive,
                Tier = GymTier.Local,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 9. Active Premium in Sydney
            new Gym
            {
                Name = "Harbour Fit",
                Address = new Address("200 George St", "Level 4", "Sydney", "NSW", "2000", "AU"),
                Status = GymStatus.Active,
                Tier = GymTier.Premium,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 10. Active MidRange in Austin
            new Gym
            {
                Name = "Lone Star Lifts",
                Address = new Address("1200 Congress Ave", null, "Austin", "TX", "78701", "US"),
                Status = GymStatus.Active,
                Tier = GymTier.MidRange,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 11. Active Elite in Tokyo
            new Gym
            {
                Name = "Tokyo Strength Club",
                Address = new Address("1-1 Chiyoda", null, "Tokyo", null, "100-8111", "JP"),
                Status = GymStatus.Active,
                Tier = GymTier.Elite,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 12. Active Local in Dublin
            new Gym
            {
                Name = "Clover Crossfit",
                Address = new Address("5 O'Connell Street", null, "Dublin", null, "D01", "IE"),
                Status = GymStatus.Active,
                Tier = GymTier.Local,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 13. Active Premium in Miami
            new Gym
            {
                Name = "Ocean Drive Wellness",
                Address = new Address("10 Ocean Dr", null, "Miami Beach", "FL", "33139", "US"),
                Status = GymStatus.Active,
                Tier = GymTier.Premium,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 14. Suspended MidRange
            new Gym
            {
                Name = "Fraud Fitness",
                Address = new Address("99 Sketchy Lane", null, "Las Vegas", "NV", "89109", "US"),
                Status = GymStatus.Suspended,
                Tier = GymTier.MidRange,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 15. Active Local in Rome
            new Gym
            {
                Name = "Colosseum Gym",
                Address = new Address("Via del Corso 10", null, "Rome", null, "00186", "IT"),
                Status = GymStatus.Active,
                Tier = GymTier.Local,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 16. Active Elite in Dubai
            new Gym
            {
                Name = "Burj Fitness",
                Address = new Address("Sheikh Zayed Rd", "Floor 45", "Dubai", null, "00000", "AE"),
                Status = GymStatus.Active,
                Tier = GymTier.Elite,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 17. Inactive Premium
            new Gym
            {
                Name = "Closed Circuit Gym",
                Address = new Address("404 Not Found St", null, "San Francisco", "CA", "94105", "US"),
                Status = GymStatus.Inactive,
                Tier = GymTier.Premium,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 18. Active MidRange in Barcelona
            new Gym
            {
                Name = "Sol y Acero",
                Address = new Address("La Rambla 50", null, "Barcelona", null, "08002", "ES"),
                Status = GymStatus.Active,
                Tier = GymTier.MidRange,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 19. Active Local in Denver
            new Gym
            {
                Name = "Mile High Muscle",
                Address = new Address("1600 Broadway", null, "Denver", "CO", "80202", "US"),
                Status = GymStatus.Active,
                Tier = GymTier.Local,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            },

            // 20. Active Elite in Singapore
            new Gym
            {
                Name = "Marina Bay Fitness",
                Address = new Address("10 Bayfront Ave", null, "Singapore", null, "018956", "SG"),
                Status = GymStatus.Active,
                Tier = GymTier.Elite,
                CreatedOn = now,
                CreatedBy = "Seed_Script",
                LastModifiedOn = now,
                LastModifiedBy = null
            }
        };

        await _context.Gyms.AddRangeAsync(gyms);
        await _context.SaveChangesAsync();
    }
}
