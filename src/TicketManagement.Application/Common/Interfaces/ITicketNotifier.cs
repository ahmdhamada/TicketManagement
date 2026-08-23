namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// Push real-time ticket notifications to connected clients. Implemented in the API layer
/// with SignalR so the Application layer stays free of transport-level concerns.
/// </summary>
public interface ITicketNotifier
{
    Task TicketCreatedAsync(int ticketId, CancellationToken ct = default);
    Task TicketUpdatedAsync(int ticketId, CancellationToken ct = default);
}
