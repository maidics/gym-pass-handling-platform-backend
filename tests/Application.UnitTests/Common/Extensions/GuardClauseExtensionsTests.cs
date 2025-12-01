using Ardalis.GuardClauses;
using FitPass.Application.Common.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Application.UnitTests.Common.Extensions;

public class GuardClauseExtensionsTests
{
    [Test]
    public void ShouldThrowIfParameterIsNull()
    {
        string userId = Guid.NewGuid().ToString();

        GymEmployment? gymEmployment = null;

        var action = () => Guard.Against.NullEntityRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), userId);

        var ex = Should.Throw<ArgumentNullException>(action);
        ex.Message.ShouldContain($"No {nameof(GymEmployment)} found for '{userId}' user.");
    }

    [Test]
    public void ShouldNotThrowIfParameterIsNotNull()
    {
        string userId = Guid.NewGuid().ToString();

        GymEmployment gymEmployment = new GymEmployment
        {
            UserId = userId,
            GymId = Guid.NewGuid().ToString(),
            Role = Roles.GymAdministrator,
            EmploymentStart = DateTimeOffset.UtcNow
        };

        var action = () => Guard.Against.NullEntityRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), userId);

        Should.NotThrow(action);
    }
}
