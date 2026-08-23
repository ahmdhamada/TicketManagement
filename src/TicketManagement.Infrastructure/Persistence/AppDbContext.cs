using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TicketActivity> TicketActivities => Set<TicketActivity>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    EntityEntry<TEntity> IApplicationDbContext.Entry<TEntity>(TEntity entity) => Entry(entity);
}
