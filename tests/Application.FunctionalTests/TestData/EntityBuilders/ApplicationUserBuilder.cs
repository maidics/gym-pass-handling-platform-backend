using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Constants;
using FitPass.Domain.Strings;
using FitPass.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.EntityBuilders;

public class ApplicationUserBuilder : TestEntityBuilderBase<ApplicationUser>
{
    private string _id = Guid.NewGuid().ToString();
    private string _email = $"user-{Guid.NewGuid()}@localhost";
    private string _role = Roles.User;
    private string? _password;

    public ApplicationUserBuilder(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory) { }

    public ApplicationUserBuilder WithId(string id)
    {
        AssertId(id);

        _id = id;

        return this;
    }

    public ApplicationUserBuilder WithEmail(string email)
    {
        AssertEmail(email);

        _email = email;

        return this;
    }

    public ApplicationUserBuilder WithPassword(string password)
    {
        if (password.Length < 8)
        {
            throw new InvalidOperationException(ErrorMessages.PasswordMinimumLength());
        }

        if (password.Length > MaxStringLengths.Password)
        {
            throw new InvalidOperationException(ErrorMessages.PropertyCannotBeLongerThan(nameof(password), MaxStringLengths.Password));
        }

        if (!password.Any(char.IsLower))
        {
            throw new InvalidOperationException(ErrorMessages.PasswordAtLeastOneLowerCase());
        }

        if (!password.Any(char.IsUpper))
        {
            throw new InvalidOperationException(ErrorMessages.PasswordAtLeastOneUpperCase());
        }

        if (!password.Any(char.IsDigit))
        {
            throw new InvalidOperationException(ErrorMessages.PasswordAtLeastOneNumber());
        }

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
        {
            throw new InvalidOperationException(ErrorMessages.PasswordAtLeastOneSpecial());
        }

        _password = password;

        return this;
    }

    public ApplicationUserBuilder WithRole(string role)
    {
        if (!Roles.IsValidRole(role))
        {
            throw new InvalidOperationException($"'{role}' is not a valid role.");
        }

        _role = role;

        return this;
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }

    public override ApplicationUser Build()
    {
        var user = new ApplicationUser
        {
            Id = _id,
            Email = _email,
            UserName = _email
        };

        return user;
    }

    public override async Task<ApplicationUser> BuildAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = Build();

        if (_password is null)
        {
            var creationResult = await userManager.CreateAsync(user);

            AssertIdentityResult(creationResult, nameof(userManager.CreateAsync));
        } else
        {
            var creationResult = await userManager.CreateAsync(user, _password);

            AssertIdentityResult(creationResult, nameof(userManager.CreateAsync));
        }
        
        var roleResult = await userManager.AddToRoleAsync(user, _role);

        AssertIdentityResult(roleResult, nameof(userManager.AddToRoleAsync));

        var createdUser = await userManager.FindByIdAsync(user.Id);

        Guard.Against.Null(createdUser);

        return createdUser;
    }
}
