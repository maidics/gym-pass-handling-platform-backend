using Fitpass.Application.ApplicationUsers.Queries;
using FitPass.Application.Common.Behaviours;
using FitPass.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FitPass.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<GetMyUserProfileDataQuery>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<GetMyUserProfileDataQuery>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallFindUserByIdAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<GetMyUserProfileDataQuery>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new GetMyUserProfileDataQuery(), new CancellationToken());

        _identityService.Verify(i => i.FindUserByIdAsync(It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallFindUserByIdAsyncOnceIfAuthenticated()
    {
        var requestLogger = new LoggingBehaviour<GetMyUserProfileDataQuery>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new GetMyUserProfileDataQuery(), new CancellationToken());

        _identityService.Verify(i => i.FindUserByIdAsync(It.IsAny<string>(), default), Times.Never);
    }
}
