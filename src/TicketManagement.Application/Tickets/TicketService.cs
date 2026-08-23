using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Common;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Common.Models;
using TicketManagement.Application.Tickets.Dtos;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Exceptions;
using TicketManagement.Domain.Rules;

namespace TicketManagement.Application.Tickets;

public class TicketService : ITicketService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly ITicketNotifier _notifier;

    public TicketService(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock,
        ITicketNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _notifier = notifier;
    }

    public async Task<PagedResult<TicketListItemDto>> GetTicketsAsync(TicketQueryParameters query, CancellationToken ct = default)
    {
        var tickets = ApplyDataIsolation(_db.Tickets.AsNoTracking()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.TimeEntries));

        if (query.Status.HasValue)
        {
            tickets = tickets.Where(t => t.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            tickets = tickets.Where(t => t.Priority == query.Priority.Value);
        }

        if (query.AssignedToUserId.HasValue)
        {
            tickets = tickets.Where(t => t.AssignedToUserId == query.AssignedToUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            tickets = tickets.Where(t => EF.Functions.Like(t.Title, $"%{term}%") || EF.Functions.Like(t.Description, $"%{term}%"));
        }

        tickets = ApplySorting(tickets, query.SortBy);

        var totalCount = await tickets.CountAsync(ct);

        var page = Math.Max(query.Page, 1);
        var items = await tickets
            .Skip((page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => ToListItemDto(t))
            .ToListAsync(ct);

        return PagedResult<TicketListItemDto>.Create(items, totalCount, page, query.PageSize);
    }

    public async Task<TicketDetailDto> GetTicketByIdAsync(int id, CancellationToken ct = default)
    {
        var ticket = await GetAuthorizedTicketAsync(id, ct, tracking: false);
        return ToDetailDto(ticket);
    }

    public async Task<TicketDetailDto> CreateTicketAsync(CreateTicketRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();

        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            Status = TicketStatus.Open,
            CreatedByUserId = userId
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync(ct);

        AddActivity(ticket.Id, userId, ActivityType.Created, null, null, "Ticket created.");
        await _db.SaveChangesAsync(ct);

        await _notifier.TicketCreatedAsync(ticket.Id, ct);

        return await GetTicketByIdAsync(ticket.Id, ct);
    }

    public async Task<TicketDetailDto> UpdateDetailsAsync(int id, UpdateTicketDetailsRequest request, CancellationToken ct = default)
    {
        var ticket = await GetAuthorizedTicketAsync(id, ct, tracking: true);
        var userId = RequireUserId();

        var isOwner = ticket.CreatedByUserId == userId;
        var isStaff = _currentUser.Role is UserRole.Admin or UserRole.Agent;
        if (!isOwner && !isStaff)
        {
            throw new ForbiddenException("Only the ticket's creator or support staff can edit its details.");
        }

        if (_currentUser.Role == UserRole.Customer && ticket.Status != TicketStatus.Open)
        {
            throw new ConflictException("Ticket details can only be edited while the ticket is still Open.");
        }

        ApplyRowVersion(ticket, request.RowVersion);

        ticket.Title = request.Title.Trim();
        ticket.Description = request.Description.Trim();
        ticket.UpdatedAtUtc = _clock.UtcNow;

        await SaveWithConcurrencyCheckAsync(ct);
        await _notifier.TicketUpdatedAsync(ticket.Id, ct);

        return await GetTicketByIdAsync(id, ct);
    }

    public async Task<TicketDetailDto> UpdateStatusAsync(int id, UpdateTicketStatusRequest request, CancellationToken ct = default)
    {
        var ticket = await GetAuthorizedTicketAsync(id, ct, tracking: true);
        var userId = RequireUserId();

        var isAllowed = _currentUser.Role == UserRole.Customer
            ? TicketStatusRules.CanCustomerTransition(ticket.Status, request.Status)
            : TicketStatusRules.CanTransition(ticket.Status, request.Status);

        if (!isAllowed)
        {
            throw new ConflictException($"Cannot transition ticket from '{ticket.Status}' to '{request.Status}'.");
        }

        ApplyRowVersion(ticket, request.RowVersion);

        var oldStatus = ticket.Status;
        ticket.Status = request.Status;
        ticket.UpdatedAtUtc = _clock.UtcNow;

        if (request.Status == TicketStatus.Resolved)
        {
            ticket.ResolvedAtUtc = _clock.UtcNow;
        }
        else if (request.Status == TicketStatus.Closed)
        {
            ticket.ClosedAtUtc = _clock.UtcNow;
        }
        else if (request.Status == TicketStatus.InProgress && oldStatus is TicketStatus.Resolved or TicketStatus.Closed)
        {
            // Reopened: clear terminal timestamps.
            ticket.ResolvedAtUtc = null;
            ticket.ClosedAtUtc = null;
        }

        AddActivity(ticket.Id, userId, ActivityType.StatusChanged, oldStatus.ToString(), request.Status.ToString());

        await SaveWithConcurrencyCheckAsync(ct);
        await _notifier.TicketUpdatedAsync(ticket.Id, ct);

        return await GetTicketByIdAsync(id, ct);
    }

    public async Task<TicketDetailDto> UpdatePriorityAsync(int id, UpdateTicketPriorityRequest request, CancellationToken ct = default)
    {
        if (_currentUser.Role == UserRole.Customer)
        {
            throw new ForbiddenException("Customers cannot change ticket priority.");
        }

        var ticket = await GetAuthorizedTicketAsync(id, ct, tracking: true);
        var userId = RequireUserId();

        ApplyRowVersion(ticket, request.RowVersion);

        var oldPriority = ticket.Priority;
        ticket.Priority = request.Priority;
        ticket.UpdatedAtUtc = _clock.UtcNow;

        AddActivity(ticket.Id, userId, ActivityType.PriorityChanged, oldPriority.ToString(), request.Priority.ToString());

        await SaveWithConcurrencyCheckAsync(ct);
        await _notifier.TicketUpdatedAsync(ticket.Id, ct);

        return await GetTicketByIdAsync(id, ct);
    }

    public async Task<TicketDetailDto> AssignAsync(int id, AssignTicketRequest request, CancellationToken ct = default)
    {
        if (_currentUser.Role != UserRole.Admin)
        {
            throw new ForbiddenException("Only administrators can assign tickets.");
        }

        var ticket = await GetAuthorizedTicketAsync(id, ct, tracking: true);
        var userId = RequireUserId();

        if (request.AssignedToUserId.HasValue)
        {
            var assignee = await _db.Users.FindAsync(new object?[] { request.AssignedToUserId.Value }, ct);
            if (assignee is null || assignee.Role != UserRole.Agent || !assignee.IsActive)
            {
                throw new DomainException("Tickets can only be assigned to an active support agent.");
            }
        }

        ApplyRowVersion(ticket, request.RowVersion);

        var oldAssignee = ticket.AssignedToUserId?.ToString() ?? "unassigned";
        ticket.AssignedToUserId = request.AssignedToUserId;
        ticket.UpdatedAtUtc = _clock.UtcNow;

        AddActivity(ticket.Id, userId, ActivityType.AssigneeChanged, oldAssignee, request.AssignedToUserId?.ToString() ?? "unassigned");

        await SaveWithConcurrencyCheckAsync(ct);
        await _notifier.TicketUpdatedAsync(ticket.Id, ct);

        return await GetTicketByIdAsync(id, ct);
    }

    // ---- helpers -------------------------------------------------------

    /// <summary>
    /// The single choke point for customer data isolation: a Customer only ever sees
    /// tickets they created, an Agent only ever sees tickets assigned to them, and an
    /// Admin sees everything. Every query path (list + get-by-id) goes through this.
    /// </summary>
    private IQueryable<Ticket> ApplyDataIsolation(IQueryable<Ticket> query) => query.VisibleTo(_currentUser);

    private async Task<Ticket> GetAuthorizedTicketAsync(int id, CancellationToken ct, bool tracking)
    {
        var query = tracking ? _db.Tickets.AsQueryable() : _db.Tickets.AsNoTracking();
        query = query.Include(t => t.CreatedByUser).Include(t => t.AssignedToUser).Include(t => t.TimeEntries);
        query = ApplyDataIsolation(query);

        var ticket = await query.SingleOrDefaultAsync(t => t.Id == id, ct);
        if (ticket is null)
        {
            // Existing-but-not-yours and truly-not-found both surface as 404 so API
            // manipulation (guessing another customer's ticket id) leaks nothing.
            throw new NotFoundException(nameof(Ticket), id);
        }

        return ticket;
    }

    private int RequireUserId() => _currentUser.UserId ?? throw new UnauthorizedAccessException("No authenticated user.");

    private void ApplyRowVersion(Ticket ticket, string base64RowVersion)
    {
        byte[] expected;
        try
        {
            expected = Convert.FromBase64String(base64RowVersion);
        }
        catch (FormatException)
        {
            throw new ConflictException("Invalid concurrency token.");
        }

        _db.Entry(ticket).Property(t => t.RowVersion).OriginalValue = expected;
    }

    private async Task SaveWithConcurrencyCheckAsync(CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("This ticket was modified by someone else. Reload and try again.");
        }
    }

    private void AddActivity(int ticketId, int actorUserId, ActivityType type, string? oldValue, string? newValue, string? description = null)
    {
        _db.TicketActivities.Add(new TicketActivity
        {
            TicketId = ticketId,
            ActorUserId = actorUserId,
            Type = type,
            OldValue = oldValue,
            NewValue = newValue,
            Description = description
        });
    }

    private static IQueryable<Ticket> ApplySorting(IQueryable<Ticket> query, string? sortBy)
    {
        var descending = sortBy?.StartsWith('-') == true;
        var field = sortBy?.TrimStart('-').ToLowerInvariant() ?? "createdat";

        return field switch
        {
            "priority" => descending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "status" => descending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "title" => descending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "updatedat" => descending ? query.OrderByDescending(t => t.UpdatedAtUtc) : query.OrderBy(t => t.UpdatedAtUtc),
            _ => descending ? query.OrderByDescending(t => t.CreatedAtUtc) : query.OrderBy(t => t.CreatedAtUtc)
        };
    }

    private static TicketListItemDto ToListItemDto(Ticket t) => new(
        t.Id,
        t.Title,
        t.Status,
        t.Priority,
        t.CreatedByUser.FullName,
        t.AssignedToUser?.FullName,
        t.CreatedAtUtc,
        t.UpdatedAtUtc,
        t.TimeEntries.Sum(te => te.DurationMinutes));

    private static TicketDetailDto ToDetailDto(Ticket t) => new(
        t.Id,
        t.Title,
        t.Description,
        t.Status,
        t.Priority,
        t.CreatedByUserId,
        t.CreatedByUser.FullName,
        t.AssignedToUserId,
        t.AssignedToUser?.FullName,
        t.CreatedAtUtc,
        t.UpdatedAtUtc,
        t.ResolvedAtUtc,
        t.ClosedAtUtc,
        t.TimeEntries.Sum(te => te.DurationMinutes),
        Convert.ToBase64String(t.RowVersion));
}
