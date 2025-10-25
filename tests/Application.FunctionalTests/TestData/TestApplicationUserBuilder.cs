using FitPass.Application.Common.Interfaces;
using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData;

public class TestApplicationUserBuilder : TestEntityBuilderBase<ApplicationUser>
{
    private readonly string _userId = Guid.NewGuid().ToString();

    private readonly ApplicationUser _user;
    private string? _role = null;

    public TestApplicationUserBuilder(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory) 
    {
        _user = new()
        {
            Id = _userId,
            FirstName = "Test",
            LastName = "User",
            Email = $"user_{_userId}@localhost",
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

    public TestApplicationUserBuilder WithGymStaffAssignment(GymStaffAssignment gymStaffAssignment)
    {
        _user.GymStaffAssignment = gymStaffAssignment;

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
}
