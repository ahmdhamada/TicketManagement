using TicketManagement.Application.TimeEntries.Dtos;

namespace TicketManagement.Application.TimeEntries;

public interface ITimeEntryService
{
    Task<TicketTimeSummaryDto> GetForTicketAsync(int ticketId, CancellationToken ct = default);
    Task<TimeEntryDto> LogTimeAsync(int ticketId, CreateTimeEntryRequest request, CancellationToken ct = default);
}
