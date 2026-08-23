using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Activities.Dtos;
using TicketManagement.Application.Common;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Application.Activities;

public class ActivityService : IActivityService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ActivityService(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TicketActivityDto>> GetTimelineAsync(int ticketId, CancellationToken ct = default)
    {
        var visible = await _db.Tickets.VisibleTo(_currentUser).AnyAsync(t => t.Id == ticketId, ct);
        if (!visible)
        {
            throw new NotFoundException(nameof(Ticket), ticketId);
        }

        return await _db.TicketActivities.AsNoTracking()
            .Include(a => a.ActorUser)
            .Where(a => a.TicketId == ticketId)
            .OrderBy(a => a.CreatedAtUtc)
            .Select(a => new TicketActivityDto(a.Id, a.TicketId, a.ActorUserId, a.ActorUser.FullName, a.Type, a.OldValue, a.NewValue, a.Description, a.CreatedAtUtc))
            .ToListAsync(ct);
    }
}
