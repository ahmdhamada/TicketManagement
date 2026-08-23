using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Common;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.TimeEntries.Dtos;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Application.TimeEntries;

/// <summary>Only Admins and Agents log billable/work time; customers never do.</summary>
public class TimeEntryService : ITimeEntryService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public TimeEntryService(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TicketTimeSummaryDto> GetForTicketAsync(int ticketId, CancellationToken ct = default)
    {
        await EnsureTicketVisibleAsync(ticketId, ct);

        var entries = await _db.TimeEntries.AsNoTracking()
            .Include(te => te.User)
            .Where(te => te.TicketId == ticketId)
            .OrderByDescending(te => te.WorkDate)
            .Select(te => ToDto(te))
            .ToListAsync(ct);

        return new TicketTimeSummaryDto(ticketId, entries.Sum(e => e.DurationMinutes), entries);
    }

    public async Task<TimeEntryDto> LogTimeAsync(int ticketId, CreateTimeEntryRequest request, CancellationToken ct = default)
    {
        if (_currentUser.Role == UserRole.Customer)
        {
            throw new ForbiddenException("Customers cannot log work time.");
        }

        await EnsureTicketVisibleAsync(ticketId, ct);
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        if (request.WorkDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new DomainException("Work date cannot be in the future.");
        }

        var entry = new TimeEntry
        {
            TicketId = ticketId,
            UserId = userId,
            WorkDate = request.WorkDate,
            DurationMinutes = request.DurationMinutes,
            Description = request.Description?.Trim()
        };

        _db.TimeEntries.Add(entry);

        _db.TicketActivities.Add(new TicketActivity
        {
            TicketId = ticketId,
            ActorUserId = userId,
            Type = ActivityType.TimeLogged,
            NewValue = $"{request.DurationMinutes}m",
            Description = $"Logged {request.DurationMinutes} minute(s) on {request.WorkDate:yyyy-MM-dd}."
        });

        await _db.SaveChangesAsync(ct);

        var user = await _db.Users.AsNoTracking().SingleAsync(u => u.Id == userId, ct);
        return new TimeEntryDto(entry.Id, ticketId, userId, user.FullName, entry.WorkDate, entry.DurationMinutes, entry.Description, entry.CreatedAtUtc);
    }

    private async Task<Ticket> EnsureTicketVisibleAsync(int ticketId, CancellationToken ct)
    {
        var ticket = await _db.Tickets.VisibleTo(_currentUser).SingleOrDefaultAsync(t => t.Id == ticketId, ct);
        return ticket ?? throw new NotFoundException(nameof(Ticket), ticketId);
    }

    private static TimeEntryDto ToDto(TimeEntry te) =>
        new(te.Id, te.TicketId, te.UserId, te.User.FullName, te.WorkDate, te.DurationMinutes, te.Description, te.CreatedAtUtc);
}
