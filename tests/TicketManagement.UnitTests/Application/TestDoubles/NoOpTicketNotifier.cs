using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.UnitTests.Application.TestDoubles;

public class NoOpTicketNotifier : ITicketNotifier
{
    public Task TicketCreatedAsync(int ticketId, CancellationToken ct = default) => Task.CompletedTask;
    public Task TicketUpdatedAsync(int ticketId, CancellationToken ct = default) => Task.CompletedTask;
}
