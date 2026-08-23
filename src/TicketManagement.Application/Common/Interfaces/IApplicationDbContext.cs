using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext so the Application layer can depend on
/// persistence without referencing Infrastructure or EF Core provider packages directly.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<Comment> Comments { get; }
    DbSet<TicketActivity> TicketActivities { get; }
    DbSet<TimeEntry> TimeEntries { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Exposes change-tracker entry access (e.g. to set an original RowVersion for optimistic-concurrency checks).</summary>
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
