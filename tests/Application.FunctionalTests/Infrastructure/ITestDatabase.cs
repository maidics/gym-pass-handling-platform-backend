using System.Data.Common;

namespace FitPass.Application.FunctionalTests.Infrastructure;

public interface ITestDatabase
{
    Task InitialiseAsync();

    DbConnection GetConnection();

    string GetConnectionString();

    Task ResetAsync();

    Task DisposeAsync();
}
