using TicketManagement.Application.Dashboard.Dtos;

namespace TicketManagement.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}
