using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.EntityBuilders;

public class UserProfileBuilder : TestEntityBuilderBase<UserProfile>
{
    private string _applicationUserId = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    public UserProfileBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory) { }

    public UserProfileBuilder WithApplicationUserId(string applicationUserId)
    {
        AssertId(applicationUserId);

        _applicationUserId = applicationUserId;

        return this;
    }

    public UserProfileBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;

        return this;
    }

    public UserProfileBuilder WithLastName(string lastName)
    {
        _lastName = lastName;

        return this;
    }

    public override UserProfile Build()
    {
        var userProfile = new UserProfile
        {
            ApplicationUserId = _applicationUserId,
            FirstName = _firstName,
            LastName = _lastName
        };

        return userProfile;
    }

    public override async Task<UserProfile> BuildAsync()
    {
        var userProfile = Build();

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Guard.Against.NullOrEmpty(_applicationUserId);

        await context.UserProfiles.AddAsync(userProfile);
        await context.SaveChangesAsync();

        var createdUserProfile = await context
            .UserProfiles
            .FindAsync(userProfile.ApplicationUserId);

        Guard.Against.Null(createdUserProfile);

        return createdUserProfile;
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }
}
