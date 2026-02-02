using FitPass.Domain.Constants;
using FitPass.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.Infrastructure.Testing;

public partial class Testing
{
    public static async Task<ApplicationUser> CreateUserAsync(
        string? email = default,
        string? password = "Password123!",
        string role = Roles.User,
        bool emailConfirmed = false
    )
    {
        if (email is null)
        {
            email = $"{Guid.NewGuid()}@test";
        }

        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = emailConfirmed,
        };

        if (password is null)
        {
            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }
        }
        else
        {
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }
        }

        if (!Roles.IsValidRole(role))
        {
            throw new Exception($"Invalid role: {role}");
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, role);
        if (!addToRoleResult.Succeeded)
        {
            throw new Exception(
                $"Failed to add user to role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}"
            );
        }

        return user;
    }
}
