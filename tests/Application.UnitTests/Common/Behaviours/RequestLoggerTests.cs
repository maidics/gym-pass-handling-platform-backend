using FitPass.Application.Common.Interfaces;
using FitPass.Application.Users.Queries;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FitPass.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<GetMyUserQuery>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<GetMyUserQuery>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    /*
    [Test]
    public async Task ShouldCallFindUserByIdAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<GetMyUserProfileQuery>(_logger.Object, _user.Object);

        await requestLogger.Process(new GetMyUserProfileQuery(), new CancellationToken());

        _identityService.Verify(i => i.DoesUserExist(It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallDoesUserExistOnceIfAuthenticated()
    {
        var requestLogger = new LoggingBehaviour<GetMyUserProfileQuery>(_logger.Object, _user.Object);

        await requestLogger.Process(new GetMyUserProfileQuery(), new CancellationToken());

        _identityService.Verify(i => i.DoesUserExist(It.IsAny<string>(), default), Times.Never);
    }
    */
}
