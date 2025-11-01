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
    public abstract TNavigationProperty GetNavigationProperty<TNavigationProperty>() where TNavigationProperty : class;
    protected abstract void AssertEntity();
}
