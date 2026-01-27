
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.GymPassProducts.Commands;
using FitPass.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FitPass.Web.Endpoints;

public class ServerSentEvents : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(SendUserEvents).RequireAuthorization();

        groupBuilder.MapPost(TestSSE, "Test").RequireAuthorization();
    }

    public ServerSentEventsResult<ClientNotification> SendUserEvents(IUser user, IClientNotificationStreamer streamer, CancellationToken cancellationToken)
    {
        return TypedResults.ServerSentEvents(streamer.StreamUserUpdates(user.Id!, cancellationToken));
    }

    public async Task<NoContent> TestSSE(IClientNotificationSender sender, IUser user, ILocalizer localizer)
    {
        var notification = ClientNotification.Create(
            localizer.Get(nameof(SharedResource.RequiresStripeAccount)),
            ClientNotificationType.Default);

        await sender.SendAsync(user.Id!, notification);

        return TypedResults.NoContent();
    }
}
