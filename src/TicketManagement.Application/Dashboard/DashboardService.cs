using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Common;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Dashboard.Dtos;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Dashboard;

/// <summary>
/// Aggregate stats scoped by the same role-based visibility as tickets: an Agent's
/// dashboard reflects only their own workload, a Customer's reflects only their own
/// tickets, and Admin sees the organization-wide picture. Results are cached briefly
/// (see ICacheService usage in the API layer) since these are expensive aggregate queries.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var tickets = _db.Tickets.AsNoTracking().VisibleTo(_currentUser);

        var byStatusRaw = await tickets.GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var byPriorityRaw = await tickets.GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        int CountOf(TicketStatus s) => byStatusRaw.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

        var openCritical = await tickets.CountAsync(
            t => t.Priority == TicketPriority.Critical && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved, ct);

        var resolvedWithTimestamps = await tickets
            .Where(t => t.ResolvedAtUtc != null)
            .Select(t => new { t.CreatedAtUtc, ResolvedAtUtc = t.ResolvedAtUtc!.Value })
            .ToListAsync(ct);

        var avgResolutionHours = resolvedWithTimestamps.Count == 0
            ? 0
            : resolvedWithTimestamps.Average(t => (t.ResolvedAtUtc - t.CreatedAtUtc).TotalHours);

        // Agent workload is org-wide operational data — only Admins get to see it.
        var agentWorkload = _currentUser.Role == UserRole.Admin
            ? await _db.Users.AsNoTracking()
                .Where(u => u.Role == UserRole.Agent && u.IsActive)
                .Select(a => new AgentWorkloadDto(
                    a.Id,
                    a.FullName,
                    a.AssignedTickets.Count(t => t.Status == TicketStatus.Open),
                    a.AssignedTickets.Count(t => t.Status == TicketStatus.InProgress),
                    a.AssignedTickets.Count(),
                    a.TimeEntries.Sum(te => (int?)te.DurationMinutes) ?? 0))
                .ToListAsync(ct)
            : new List<AgentWorkloadDto>();

        return new DashboardSummaryDto(
            TotalTickets: byStatusRaw.Sum(x => x.Count),
            OpenTickets: CountOf(TicketStatus.Open),
            InProgressTickets: CountOf(TicketStatus.InProgress),
            ResolvedTickets: CountOf(TicketStatus.Resolved),
            ClosedTickets: CountOf(TicketStatus.Closed),
            OpenCriticalTickets: openCritical,
            AverageResolutionHours: Math.Round(avgResolutionHours, 1),
            ByStatus: byStatusRaw.Select(x => new StatusCountDto(x.Status.ToString(), x.Count)).ToList(),
            ByPriority: byPriorityRaw.Select(x => new PriorityCountDto(x.Priority.ToString(), x.Count)).ToList(),
            AgentWorkload: agentWorkload);
    }
}
