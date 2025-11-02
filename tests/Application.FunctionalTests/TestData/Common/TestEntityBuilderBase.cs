using System.ComponentModel.DataAnnotations;
using FitPass.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.Common;

public abstract class TestEntityBuilderBase<TEntity> : ITestEntityBuilder<TEntity> where TEntity : class
{
    protected readonly IServiceScopeFactory _scopeFactory;

    public TestEntityBuilderBase(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public abstract TEntity Build();
    public abstract Task<TEntity> BuildAsync();
    protected abstract void AssertEntity();

    protected void AssertId(string? id)
    {
        if (string.Empty == id || string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException($"Given id '{id}' is not valid.");
        }
    }

    protected void AssertEmail(string email)
    {
        var emailAttribute = new EmailAddressAttribute();

        if (string.IsNullOrEmpty(email) || !emailAttribute.IsValid(email))
        {
            throw new InvalidOperationException($"'{email}' email is not valid.");
        }
    }

    protected void AssertRole(string role)
    {
        if (!Roles.IsValidRole(role))
        {
            throw new InvalidOperationException($"'{role}' is not valid.");
        }
    }

    protected void AssertIdentityResult(IdentityResult? identityResult, string methodName)
    {
        if (identityResult == null || !identityResult.Succeeded || identityResult.Errors.Any())
        {
            throw new InvalidOperationException($"'{methodName}' failed. Errors: {identityResult?.Errors.Select(e => e.Description)}");
        }
    }
}
