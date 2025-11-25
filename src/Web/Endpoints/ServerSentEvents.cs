
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class ServerSentEvents : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(SendUserEvents).RequireAuthorization();
    }

    public ServerSentEventsResult<ClientNotification> SendUserEvents(IUser user, IClientNotificationStreamer streamer, CancellationToken cancellationToken)
    {
        return TypedResults.ServerSentEvents(streamer.StreamUserUpdates(user.Id!, cancellationToken));
    }
}
