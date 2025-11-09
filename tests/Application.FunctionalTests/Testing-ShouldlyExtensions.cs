using System.Reflection;
using FitPass.Application.Common.Security;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public static void ShouldNotRequireAuthorization<TRequest>()
    {
        var type = typeof(TRequest);

        var attributes = type.GetCustomAttributes<AuthorizeAttribute>();

        attributes.Any().ShouldBeFalse();
    }

    public static void ShouldRequireAuthorization<TRequest>(params string[] expectedRoles) where TRequest : notnull
    {
        var type = typeof(TRequest);

        var attributes = type.GetCustomAttributes<AuthorizeAttribute>();

        attributes.Any().ShouldBeTrue(
            $"{type.Name} should have at least one [Authorize] attribute"
        );

        var actualRoles = attributes
            .Where(attr => !string.IsNullOrWhiteSpace(attr.Roles))
            .SelectMany(attr => ParseRoles(attr.Roles!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var cleanedExpectedRoles = expectedRoles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (cleanedExpectedRoles.Count == 0)
        {
            actualRoles.Count.ShouldBe(0,
                $"{type.Name} should not require any specific roles, but requires: [{string.Join(", ", actualRoles)}]"
            );
            return;
        }

        var hasEmptyRoles = attributes.Any(attr => string.IsNullOrWhiteSpace(attr.Roles));
        hasEmptyRoles.ShouldBeFalse(
            $"{type.Name} has [Authorize] without roles, which allows any authenticated user. " +
            $"Expected specific roles: [{string.Join(", ", cleanedExpectedRoles)}]"
        );

        actualRoles.ShouldBe(cleanedExpectedRoles,
            customMessage: $"{type.Name} authorization roles mismatch"
        );
    }

    private static IEnumerable<string> ParseRoles(string rolesString)
    {
        return rolesString
            .Split(',')
            .Select(role => role.Trim())
            .Where(role => !string.IsNullOrEmpty(role));
    }
}
