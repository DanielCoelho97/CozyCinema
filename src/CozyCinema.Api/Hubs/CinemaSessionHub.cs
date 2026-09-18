using CozyCinema.Application.RealTime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CozyCinema.Api.Hubs;

[Authorize]
public class CinemaSessionHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var sessionIdValue = Context.GetHttpContext()?.Request.Query["sessionId"].ToString();

        if (Guid.TryParse(sessionIdValue, out var sessionId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, SessionHubEvents.GroupName(sessionId));
        }

        await base.OnConnectedAsync();
    }
}
