using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Comments.Dtos;
using TicketManagement.Application.Common;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Application.Comments;

public class CommentService : ICommentService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ITicketNotifier _notifier;

    public CommentService(IApplicationDbContext db, ICurrentUserService currentUser, ITicketNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<IReadOnlyList<CommentDto>> GetCommentsForTicketAsync(int ticketId, CancellationToken ct = default)
    {
        await EnsureTicketVisibleAsync(ticketId, ct);

        return await _db.Comments.AsNoTracking()
            .Include(c => c.AuthorUser)
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAtUtc)
            .Select(c => new CommentDto(c.Id, c.TicketId, c.AuthorUserId, c.AuthorUser.FullName, c.Body, c.CreatedAtUtc))
            .ToListAsync(ct);
    }

    public async Task<CommentDto> AddCommentAsync(int ticketId, CreateCommentRequest request, CancellationToken ct = default)
    {
        var ticket = await EnsureTicketVisibleAsync(ticketId, ct);
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var comment = new Comment
        {
            TicketId = ticketId,
            AuthorUserId = userId,
            Body = request.Body.Trim()
        };
        _db.Comments.Add(comment);

        _db.TicketActivities.Add(new TicketActivity
        {
            TicketId = ticketId,
            ActorUserId = userId,
            Type = ActivityType.CommentAdded,
            Description = "Comment added."
        });

        ticket.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await _notifier.TicketUpdatedAsync(ticketId, ct);

        var author = await _db.Users.AsNoTracking().SingleAsync(u => u.Id == userId, ct);
        return new CommentDto(comment.Id, ticketId, userId, author.FullName, comment.Body, comment.CreatedAtUtc);
    }

    /// <summary>Applies the same role-based data isolation as tickets: 404s rather than 403s to avoid confirming existence.</summary>
    private async Task<Ticket> EnsureTicketVisibleAsync(int ticketId, CancellationToken ct)
    {
        var ticket = await _db.Tickets.VisibleTo(_currentUser).SingleOrDefaultAsync(t => t.Id == ticketId, ct);
        return ticket ?? throw new NotFoundException(nameof(Ticket), ticketId);
    }
}
