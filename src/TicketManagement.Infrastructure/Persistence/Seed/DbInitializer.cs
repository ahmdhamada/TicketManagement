using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Infrastructure.Persistence.Seed;

/// <summary>
/// Applies pending migrations and seeds a baseline set of users/tickets so the app is
/// usable immediately after first run. Safe to call every startup: it no-ops if data exists.
/// </summary>
public static class DbInitializer
{
    /// <summary>Applies pending EF Core migrations (or creates the schema directly if none are
    /// shipped yet), then seeds baseline data. Used by the real app on startup.</summary>
    public static async Task InitializeAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        // GetMigrations() is a pure in-memory/reflection check (no DB round-trip) — it does
        // NOT create the __EFMigrationsHistory table, unlike GetPendingMigrationsAsync().
        var hasMigrations = context.Database.GetMigrations().Any();

        if (hasMigrations)
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            // Self-heal: an earlier run may have called an API that queries
            // __EFMigrationsHistory (creating that table as a side effect) before falling
            // back to EnsureCreated. EnsureCreated treats "any table already exists" as
            // "schema already created" and skips creating the real tables, so the app gets
            // stuck in a half-initialized database. If that stray history table is the ONLY
            // table present, drop it before creating the schema from the model.
            await context.Database.ExecuteSqlRawAsync(
                "IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NOT NULL " +
                "AND NOT EXISTS (SELECT 1 FROM sys.tables WHERE name <> '__EFMigrationsHistory') " +
                "DROP TABLE [__EFMigrationsHistory];");

            await context.Database.EnsureCreatedAsync();
        }

        await SeedAsync(context, passwordHasher);
    }

    /// <summary>
    /// Seeds baseline data only, without touching migrations. Used by integration tests,
    /// which build their schema with EnsureCreated() against a throwaway SQLite database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var admin = new User { FullName = "Ahmed Hamada", Email = "enghamadafci99@gmail.com", Role = UserRole.Admin, PasswordHash = passwordHasher.Hash("Passw0rd!") };
        var agent1 = new User { FullName = "Samy", Email = "agent1@invento.gmail", Role = UserRole.Agent, PasswordHash = passwordHasher.Hash("Passw0rd!") };
        var agent2 = new User { FullName = "Nora", Email = "agent2@invento.gmail.com", Role = UserRole.Agent, PasswordHash = passwordHasher.Hash("Passw0rd!") };
        var customer1 = new User { FullName = "Sella Customer", Email = "customer1@gmail.com", Role = UserRole.Customer, PasswordHash = passwordHasher.Hash("Passw0rd!") };
        var customer2 = new User { FullName = "Heba Customer", Email = "customer2@gmail.com", Role = UserRole.Customer, PasswordHash = passwordHasher.Hash("Passw0rd!") };

        context.Users.AddRange(admin, agent1, agent2, customer1, customer2);
        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;

        var t1 = new Ticket
        {
            Title = "Cannot reset my password",
            Description = "The reset-password email never arrives.",
            Status = TicketStatus.Open,
            Priority = TicketPriority.High,
            CreatedByUserId = customer1.Id
        };

        var t2 = new Ticket
        {
            Title = "Invoice shows wrong total",
            Description = "Invoice #4821 totals $50 more than the quoted price.",
            Status = TicketStatus.InProgress,
            Priority = TicketPriority.Critical,
            CreatedByUserId = customer2.Id,
            AssignedToUserId = agent1.Id
        };

        var t3 = new Ticket
        {
            Title = "Feature request: dark mode",
            Description = "Would love a dark theme option in settings.",
            Status = TicketStatus.Resolved,
            Priority = TicketPriority.Low,
            CreatedByUserId = customer1.Id,
            AssignedToUserId = agent2.Id,
            ResolvedAtUtc = now.AddHours(-2)
        };

        context.Tickets.AddRange(t1, t2, t3);
        await context.SaveChangesAsync();

        context.TicketActivities.AddRange(
            new TicketActivity { TicketId = t1.Id, ActorUserId = customer1.Id, Type = ActivityType.Created, Description = "Ticket created." },
            new TicketActivity { TicketId = t2.Id, ActorUserId = customer2.Id, Type = ActivityType.Created, Description = "Ticket created." },
            new TicketActivity { TicketId = t2.Id, ActorUserId = admin.Id, Type = ActivityType.AssigneeChanged, OldValue = "unassigned", NewValue = agent1.Id.ToString() },
            new TicketActivity { TicketId = t3.Id, ActorUserId = customer1.Id, Type = ActivityType.Created, Description = "Ticket created." },
            new TicketActivity { TicketId = t3.Id, ActorUserId = agent2.Id, Type = ActivityType.StatusChanged, OldValue = "InProgress", NewValue = "Resolved" }
        );

        context.TimeEntries.AddRange(
            new TimeEntry { TicketId = t2.Id, UserId = agent1.Id, WorkDate = DateOnly.FromDateTime(now.AddDays(-1)), DurationMinutes = 45, Description = "Investigated billing discrepancy." },
            new TimeEntry { TicketId = t3.Id, UserId = agent2.Id, WorkDate = DateOnly.FromDateTime(now.AddDays(-3)), DurationMinutes = 90, Description = "Implemented dark theme prototype." }
        );

        context.Comments.AddRange(
            new Comment { TicketId = t2.Id, AuthorUserId = agent1.Id, Body = "Looking into this now, will update shortly." },
            new Comment { TicketId = t3.Id, AuthorUserId = agent2.Id, Body = "Shipped in the latest release, please verify." }
        );

        await context.SaveChangesAsync();
    }
}
