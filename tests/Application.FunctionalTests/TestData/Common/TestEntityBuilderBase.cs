using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
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
}
