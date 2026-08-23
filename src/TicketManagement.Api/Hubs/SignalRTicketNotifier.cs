using Microsoft.AspNetCore.SignalR;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Api.Hubs;

public class SignalRTicketNotifier : ITicketNotifier
{
    private readonly IHubContext<TicketHub> _hubContext;

    public SignalRTicketNotifier(IHubContext<TicketHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task TicketCreatedAsync(int ticketId, CancellationToken ct = default) =>
        _hubContext.Clients.Group(TicketHub.GroupName).SendAsync("ticketCreated", ticketId, cancellationToken: ct);

    public Task TicketUpdatedAsync(int ticketId, CancellationToken ct = default) =>
        _hubContext.Clients.Group(TicketHub.GroupName).SendAsync("ticketUpdated", ticketId, cancellationToken: ct);
}
