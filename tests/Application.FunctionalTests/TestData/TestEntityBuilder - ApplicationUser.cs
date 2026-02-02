using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.TestData;

using static Testing;

public static partial class TestEntityBuilder
{
    public static async Task<(
        ApplicationUser user,
        UserProfile userProfile
    )> BuildDefaultUserAsync()
    {
        var user = await CreateUserAsync();

        var userProfile = new UserProfile
        {
            UserId = user.Id,
            FirstName = "Default",
            LastName = "User",
            PreferredLanguage = GetDefaultCulture(),
            CreatedOn = GetUtcNow(),
        };

        await AddAsync(userProfile);

        return (user, userProfile);
    }

    public static async Task<(
        ApplicationUser user,
        UserProfile userProfile
    )> BuildPendingGymEmployeeAsync(bool emailConfirmed = false)
    {
        var user = await CreateUserAsync(
            role: Roles.PendingGymEmployee,
            emailConfirmed: emailConfirmed
        );

        var userProfile = new UserProfile
        {
            UserId = user.Id,
            FirstName = "Pending",
            LastName = "GymEmployee",
            PreferredLanguage = GetDefaultCulture(),
            CreatedOn = GetUtcNow(),
        };

        await AddAsync(userProfile);

        return (user, userProfile);
    }

    public static async Task<(ApplicationUser user, UserProfile userProfile)> BuildAppAdminAsync()
    {
        var user = await CreateUserAsync(role: Roles.AppAdministrator);

        var userProfile = new UserProfile
        {
            UserId = user.Id,
            FirstName = "App",
            LastName = "Admin",
            PreferredLanguage = GetDefaultCulture(),
            CreatedOn = GetUtcNow(),
        };

        await AddAsync(userProfile);

        return (user, userProfile);
    }

    public static async Task<(
        ApplicationUser user,
        Gym gym,
        GymEmployment gymEmployment,
        UserProfile userProfile
    )> BuildGymEmployeeAsync(
        string employeeRole,
        GymStatus gymStatus = GymStatus.Active,
        List<GymContactInfo>? gymContactInfos = null
    )
    {
        if (employeeRole != Roles.GymAdministrator && employeeRole != Roles.GymStaff)
        {
            throw new InvalidOperationException("$'{role}' role is not a gym employee role.");
        }

        var user = await CreateUserAsync(role: employeeRole);

        var gym = new Gym
        {
            Name = $"Test Gym - {Guid.NewGuid()}",
            Address = new Address("line1", "line2", "city", null, "postalCode", "HU"),
            Status = gymStatus,
            Tier = GymTier.Local,
            ContactInfos = gymContactInfos ?? [],
        };

        await AddAsync(gym);

        var gymEmployment = new GymEmployment
        {
            UserId = user.Id,
            GymId = gym.Id,
            Role = employeeRole,
        };

        await AddAsync(gymEmployment);

        var userProfile = new UserProfile
        {
            UserId = user.Id,
            FirstName = "Gym",
            LastName = "Employee",
            PreferredLanguage = GetDefaultCulture(),
            CreatedOn = GetUtcNow(),
        };

        await AddAsync(userProfile);

        return (user, gym, gymEmployment, userProfile);
    }
}
