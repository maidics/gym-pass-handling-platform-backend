namespace FitPass.Application.FunctionalTests.Tests;

using static Testing;

[TestFixture]
public abstract class BaseTestFixture
{
    [SetUp]
    public async Task TestSetUp()
    {
        await ResetState();
    }

    public abstract void AuthorizeAttributeCheck();
}
