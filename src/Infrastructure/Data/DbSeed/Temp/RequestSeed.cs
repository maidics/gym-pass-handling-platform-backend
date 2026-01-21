using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private async Task SeedRequestsAsync()
    {
        var now = DateTimeOffset.UtcNow;

        var requests = new List<Request>()
        {
            new Request
            {
                CreatedOn = now,
                CreatedBy = "PendingGymEmployeeId",
                LastModifiedOn = now,
                LastModifiedBy = "PendingGymEmployeeId",
                Title = "LONG TITLE - AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                Description = "Opening a new flagship location in the business district. AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                PriorityLevel = PriorityLevel.High,
                Type = RequestType.GymCreation,
                Status = RequestStatus.Submitted,
                Payload = """{"Name": "Titan Fitness", "Address": {"Line1": "100 Wall St", "Line2": "Floor 1", "City": "New York", "State": "NY", "PostalCode": "10005", "CountryAlpha2": "US"}, "Status": 0, "Tier": 3, "SupervisorEmail": "ceo@titanfit.com"}"""
            },
            
            new Request
            {
                CreatedOn = now,
                CreatedBy = "PendingGymEmployeeId",
                LastModifiedOn = now,
                LastModifiedBy = "PendingGymEmployeeId",
                Title = "Register Titan Fitness",
                Description = "Opening a new flagship location in the business district.",
                PriorityLevel = PriorityLevel.High,
                Type = RequestType.GymCreation,
                Status = RequestStatus.Submitted,
                Payload = """{"Name": "Titan Fitness", "Address": {"Line1": "100 Wall St", "Line2": "Floor 1", "City": "New York", "State": "NY", "PostalCode": "10005", "CountryAlpha2": "US"}, "Status": 0, "Tier": 3, "SupervisorEmail": "ceo@titanfit.com"}"""
            },

            new Request
            {
                CreatedOn = now.AddDays(-3),
                CreatedBy = "PendingGymEmployeeId",
                LastModifiedOn = now.AddDays(-1),
                LastModifiedBy = "AppAdminLocalhostId",
                Title = "Register Joe's Garage",
                Description = "Small local gym setup.",
                PriorityLevel = PriorityLevel.Medium,
                Type = RequestType.GymCreation,
                Status = RequestStatus.Rejected,
                HandlerRationale = "Address validation failed. \"Garage\" is not a valid commercial zone.",
                Payload = """{"Name": "Joe's Garage", "Address": {"Line1": "12 Back Alley", "Line2": null, "City": "Chicago", "State": "IL", "PostalCode": "60601", "CountryAlpha2": "US"}, "Status": 0, "Tier": 0, "SupervisorEmail": "joe@garage.com"}"""
            },

            new Request
            {
                CreatedOn = now.AddHours(-5),
                CreatedBy = "PendingGymEmployeeId",
                LastModifiedOn = now,
                LastModifiedBy = "PendingGymEmployeeId",
                Title = "Register SeaSide Wellness",
                Description = "Luxury wellness center onboarding.",
                PriorityLevel = PriorityLevel.High,
                Type = RequestType.GymCreation,
                Status = RequestStatus.Error,
                Error = "System.Data.DbUpdateException: Unique constraint violation on Index_GymName.",
                Payload = """{"Name": "SeaSide Wellness", "Address": {"Line1": "1 Ocean Dr", "Line2": null, "City": "Miami", "State": "FL", "PostalCode": "33101", "CountryAlpha2": "US"}, "Status": 0, "Tier": 2, "SupervisorEmail": "admin@seaside.com"}"""
            },

            new Request
            {
                CreatedOn = now.AddDays(-10),
                CreatedBy = "PendingGymEmployeeId",
                LastModifiedOn = now.AddDays(-9),
                LastModifiedBy = "AppAdminLocalhostId",
                Title = "Register Metro Flex",
                Description = "Mid-range city gym.",
                PriorityLevel = PriorityLevel.Medium,
                Type = RequestType.GymCreation,
                Status = RequestStatus.Approved,
                HandlerRationale = "Gym successfully provisioned and admin assigned.",
                Payload = """{"Name": "Metro Flex", "Address": {"Line1": "55 Main St", "Line2": null, "City": "Seattle", "State": "WA", "PostalCode": "98101", "CountryAlpha2": "US"}, "Status": 0, "Tier": 1, "SupervisorEmail": "contact@metroflex.com"}"""
            },

            // GYM ADMIN PROMOTION REQUESTS

            new Request
            {
                CreatedOn = now,
                CreatedBy = "GymAdminLocalhostId",
                LastModifiedOn = now,
                LastModifiedBy = "GymAdminLocalhostId",
                Title = "Promote Sarah Connor",
                Description = "Promoting lead trainer to assistant manager role.",
                PriorityLevel = PriorityLevel.Medium,
                Type = RequestType.GymAdminPromotion,
                Status = RequestStatus.Submitted,
                Payload = """{"GymId": "gym-001-guid", "UserIdToNominate": "GymStaffLocalhostId", "SupervisorEmail": "sarah.c@gym.com"}"""
            },

            new Request
            {
                CreatedOn = now.AddDays(-1),
                CreatedBy = "GymAdminLocalhostId",
                LastModifiedOn = now,
                LastModifiedBy = "GymAdminLocalhostId",
                Title = "Promote John Smith",
                Description = "Nomination for admin rights.",
                PriorityLevel = PriorityLevel.Low,
                Type = RequestType.GymAdminPromotion,
                Status = RequestStatus.Cancelled,
                HandlerRationale = "Employee resigned before promotion could be processed.",
                Payload = """{"GymId": "gym-001-guid", "UserIdToNominate": "user-guid-temp", "SupervisorEmail": "john.s@gym.com"}"""
            },

            new Request
            {
                CreatedOn = now.AddDays(-2),
                CreatedBy = "GymAdminLocalhostId",
                LastModifiedOn = now.AddDays(-1),
                LastModifiedBy = "AppAdminLocalhostId",
                Title = "Promote External Contractor",
                Description = "Granting admin access to IT contractor.",
                PriorityLevel = PriorityLevel.High,
                Type = RequestType.GymAdminPromotion,
                Status = RequestStatus.Rejected,
                HandlerRationale = "External contractors cannot hold Admin roles per company policy Section 4.2.",
                Payload = """{"GymId": "gym-002-guid", "UserIdToNominate": "contractor-guid", "SupervisorEmail": "it@contractor.com"}"""
            },

            new Request
            {
                CreatedOn = now.AddDays(-20),
                CreatedBy = "GymAdminLocalhostId",
                LastModifiedOn = now.AddDays(-19),
                LastModifiedBy = "AppAdminLocalhostId",
                Title = "Promote Shift Lead",
                Description = "Night shift lead needs dashboard access.",
                PriorityLevel = PriorityLevel.Low,
                Type = RequestType.GymAdminPromotion,
                Status = RequestStatus.Approved,
                HandlerRationale = "Access granted.",
                Payload = """{"GymId": "gym-001-guid", "UserIdToNominate": "user-shift-lead", "SupervisorEmail": "nightops@gym.com"}"""
            },

            // OTHER REQUESTS

            new Request
            {
                CreatedOn = now,
                CreatedBy = "UserId",
                LastModifiedOn = now,
                LastModifiedBy = "UserId",
                Title = "App Dark Mode Request",
                Description = "Can we please get a dark mode for the mobile app?",
                PriorityLevel = PriorityLevel.Low,
                Type = RequestType.Other,
                Status = RequestStatus.Submitted,
                Payload = null
            },

            new Request
            {
                CreatedOn = now.AddDays(-4),
                CreatedBy = "GymStaffLocalhostId",
                LastModifiedOn = now.AddDays(-2),
                LastModifiedBy = "AppAdminLocalhostId",
                Title = "Missing Report Data",
                Description = "The weekly attendance report for last Tuesday is empty.",
                PriorityLevel = PriorityLevel.Medium,
                Type = RequestType.Other,
                Status = RequestStatus.Approved,
                HandlerRationale = "Data was stuck in cache. Refreshed and report is now available.",
                Payload = null
            },

            new Request
            {
                CreatedOn = now.AddHours(-2),
                CreatedBy = "UserId",
                LastModifiedOn = now,
                LastModifiedBy = "UserId",
                Title = "Double Charge on Credit Card",
                Description = "I was charged twice for my monthly subscription.",
                PriorityLevel = PriorityLevel.High,
                Type = RequestType.Other,
                Status = RequestStatus.Error,
                Error = "PaymentGatewayException: Connection timed out while verifying transaction ID.",
                Payload = null
            },

            new Request
            {
                CreatedOn = now.AddDays(-6),
                CreatedBy = "UserId",
                LastModifiedOn = now.AddDays(-5),
                LastModifiedBy = "AppAdminLocalhostId",
                Title = "Free T-Shirt",
                Description = "Send me a free t-shirt please.",
                PriorityLevel = PriorityLevel.Low,
                Type = RequestType.Other,
                Status = RequestStatus.Rejected,
                HandlerRationale = "Not a valid support request.",
                Payload = null
            },

            new Request
            {
                CreatedOn = now,
                CreatedBy = "GymStaffLocalhostId",
                LastModifiedOn = now,
                LastModifiedBy = "GymStaffLocalhostId",
                Title = "Suspicious Activity Detected",
                Description = "User with ID 5555 tried to scan into the gym 40 times in 1 minute.",
                PriorityLevel = PriorityLevel.High,
                Type = RequestType.Other,
                Status = RequestStatus.Submitted,
                Payload = null
            }
        };

        await _context.Requests.AddRangeAsync(requests);
    }
}
