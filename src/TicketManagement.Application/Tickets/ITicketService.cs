using TicketManagement.Application.Common.Models;
using TicketManagement.Application.Tickets.Dtos;

namespace TicketManagement.Application.Tickets;

public interface ITicketService
{
    Task<PagedResult<TicketListItemDto>> GetTicketsAsync(TicketQueryParameters query, CancellationToken ct = default);
    Task<TicketDetailDto> GetTicketByIdAsync(int id, CancellationToken ct = default);
    Task<TicketDetailDto> CreateTicketAsync(CreateTicketRequest request, CancellationToken ct = default);
    Task<TicketDetailDto> UpdateDetailsAsync(int id, UpdateTicketDetailsRequest request, CancellationToken ct = default);
    Task<TicketDetailDto> UpdateStatusAsync(int id, UpdateTicketStatusRequest request, CancellationToken ct = default);
    Task<TicketDetailDto> UpdatePriorityAsync(int id, UpdateTicketPriorityRequest request, CancellationToken ct = default);
    Task<TicketDetailDto> AssignAsync(int id, AssignTicketRequest request, CancellationToken ct = default);
}
