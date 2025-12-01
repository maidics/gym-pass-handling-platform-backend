using System;
using FitPass.Application.Common.Models;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Application.UnitTests.Common.Models;

public class ClientNotificationTests
{
    [Test]
    public void ShouldCreateClientNotification()
    {
        var notification1 = ClientNotification.Create("message", ClientNotificationType.Default);
        notification1.Message.ShouldBe("message");
        notification1.Type.ShouldBe(ClientNotificationType.Default);

        var notification2 = ClientNotification.Create("message", ClientNotificationType.Default, "payload");
        notification2.Message.ShouldBe("message");
        notification2.Type.ShouldBe(ClientNotificationType.Default);
        notification2.Payload.ShouldNotBeNull();
        notification2.Payload.ShouldBe("payload");
    }
}
