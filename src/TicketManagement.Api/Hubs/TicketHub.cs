using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TicketManagement.Api.Hubs;

/// <summary>
/// Real-time channel for ticket create/update notifications. Clients join the
/// "tickets" group on connect; the server never trusts client-sent data isolation —
/// this hub only pushes ticket ids, and the client re-fetches via the normal
/// (role-filtered) REST endpoints, so no protected data ever flows over the socket.
/// </summary>
[Authorize]
public class TicketHub : Hub
{
    public const string GroupName = "tickets";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName);
        await base.OnDisconnectedAsync(exception);
    }
}
