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
        var notification1 = new ClientNotification
        {
            Message = "message",
            Type = ClientNotificationType.Default,
        };
        notification1.Message.ShouldBe("message");
        notification1.Type.ShouldBe(ClientNotificationType.Default);
    }
}
