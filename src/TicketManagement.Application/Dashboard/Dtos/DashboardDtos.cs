namespace TicketManagement.Application.Dashboard.Dtos;

public record StatusCountDto(string Status, int Count);
public record PriorityCountDto(string Priority, int Count);

public record AgentWorkloadDto(int AgentId, string AgentName, int OpenCount, int InProgressCount, int TotalAssigned, int TotalMinutesLogged);

public record DashboardSummaryDto(
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int ResolvedTickets,
    int ClosedTickets,
    int OpenCriticalTickets,
    double AverageResolutionHours,
    IReadOnlyList<StatusCountDto> ByStatus,
    IReadOnlyList<PriorityCountDto> ByPriority,
    IReadOnlyList<AgentWorkloadDto> AgentWorkload);
