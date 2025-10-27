using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData;

public class TestApplicationUserBuilder : TestEntityBuilderBase<ApplicationUser>
{
    private readonly string _userId = Guid.CreateVersion7().ToString();

    private readonly ApplicationUser _user;
    private string? _role = null;

    public TestApplicationUserBuilder(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory) 
    {
        _user = new()
        {
            Id = _userId,
            FirstName = "Test",
            LastName = "User",
            Email = $"user_{(_role == null ? "default" : _role)}@{_userId}_localhost",
            UserName = $"user_{_userId}@localhost",
            GymStaffAssignment = null,
            UserGymMemberships = null,
            PaymentProfile = null
        };
    }

    public TestApplicationUserBuilder WithPaymentProfile(UserPaymentProfile paymentProfile)
    {
        _user.PaymentProfile = paymentProfile;

        return this;
    }

    public TestApplicationUserBuilder AddUserGymMembership(UserGymMembership gymMembership)
    {
        if (_user.UserGymMemberships == null)
        {
            _user.UserGymMemberships = [gymMembership];

            return this;
        }

        _user.UserGymMemberships.Add(gymMembership);

        return this;
    }

    public TestApplicationUserBuilder WithRole(string role)
    {
        _role = role;

        return this;
    }

    public TestApplicationUserBuilder WithGymStaffAssignment(string role, string? gymId)
    {
        _user.GymStaffAssignment = new GymStaffAssignment
        {
            ApplicationUserId = _user.Id,
            GymId = gymId,
            Role = role
        };

        return this;
    }

    public TestApplicationUserBuilder AddRequest(Request request)
    {
        _user.Requests.Add(request);

        return this;
    }

    public override ApplicationUser Build()
    {
        return _user;
    }

    public override async Task<ApplicationUser> BuildAsync()
    {
        using var scope = _scopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var creationResult = await userManager.CreateAsync(_user, "Password123_");

        if (!creationResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create {(_role ?? string.Empty)} user. Result: {creationResult.ToApplicationResult()}");
        }

        if (_role != null)
        {
            var roleResult = await userManager.AddToRoleAsync(_user, _role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to add user to {_role} role. Result: {creationResult.ToApplicationResult()}");
            }
        }

        var user = await userManager.FindByIdAsync(_user.Id);

        if (user == null)
        {
            throw new InvalidOperationException($"Failed to find newly created {(_role ?? string.Empty)} user.");
        }

        return user;
    }

    public override void AssertEntity()
    {
        if (_role == Roles.AppAdministrator || _role == Roles.PendingGymManagement)
        {
            if (_user.UserPaymentProfileId != null || _user.PaymentProfile != null)
            {
                throw new InvalidOperationException($"{nameof(Roles.AppAdministrator)} cannot have UserPaymentProfile.");
            }

            if (_user.UserGymMemberships != null || _user.UserGymMemberships?.Count != 0)
            {
                throw new InvalidOperationException($"{nameof(Roles.AppAdministrator)} cannot have UserGymMemberships.");
            }

            if (_user.GymStaffAssignment != null)
            {
                throw new InvalidOperationException($"{nameof(Roles.AppAdministrator)} cannot have GymStaffAssignment.");
            }
        }

        if (_role == Roles.GymAdministrator || _role == Roles.GymStaff)
        {
            if (_user.UserPaymentProfileId != null || _user.PaymentProfile != null)
            {
                throw new InvalidOperationException($"{nameof(Roles.GymAdministrator)} cannot have UserPaymentProfile.");
            }

            if (_user.UserGymMemberships != null || _user.UserGymMemberships?.Count != 0)
            {
                throw new InvalidOperationException($"{nameof(Roles.GymAdministrator)} cannot have UserGymMemberships.");
            }

            if (_user.GymStaffAssignment == null)
            {
                throw new InvalidOperationException($"{nameof(Roles.AppAdministrator)} must have GymStaffAssignment.");
            }
        }

        if (_role == null)
        {
            if (_user.UserPaymentProfileId != null || _user.PaymentProfile != null)
            {
                throw new InvalidOperationException($"Default User must have UserPaymentProfile.");
            }

            if (_user.GymStaffAssignment != null)
            {
                throw new InvalidOperationException($"Default User cannot have GymStaffAssignment.");
            }
        }
    }
}
