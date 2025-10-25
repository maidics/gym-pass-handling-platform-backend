namespace FitPass.Application.FunctionalTests.TestData.Common;

public interface ITestEntityBuilder<TEntity> where TEntity : class
{
    TEntity Build();
    Task<TEntity> BuildAsync();
}
