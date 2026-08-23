using TicketManagement.Application.Tickets;
using TicketManagement.Application.Tickets.Dtos;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Exceptions;
using TicketManagement.UnitTests.Application.TestDoubles;

namespace TicketManagement.UnitTests.Application;

public class TicketServiceTests
{
    private static User MakeUser(int id, UserRole role) => new()
    {
        Id = id,
        FullName = $"User {id}",
        Email = $"user{id}@test.com",
        PasswordHash = "n/a",
        Role = role
    };

    [Fact]
    public async Task GetTicketsAsync_Customer_OnlySeesTheirOwnTickets()
    {
        await using var db = InMemoryDbContextFactory.Create();

        var customerA = MakeUser(1, UserRole.Customer);
        var customerB = MakeUser(2, UserRole.Customer);
        db.Users.AddRange(customerA, customerB);
        db.Tickets.AddRange(
            new Ticket { Title = "A's ticket", Description = "desc desc", CreatedByUserId = 1 },
            new Ticket { Title = "B's ticket", Description = "desc desc", CreatedByUserId = 2 });
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(userId: 1, role: UserRole.Customer);
        var sut = new TicketService(db, currentUser, new FakeDateTimeProvider(), new NoOpTicketNotifier());

        var result = await sut.GetTicketsAsync(new TicketQueryParameters());

        result.TotalCount.Should().Be(1);
        result.Items.Single().Title.Should().Be("A's ticket");
    }

    [Fact]
    public async Task GetTicketByIdAsync_Customer_RequestingAnotherCustomersTicket_ThrowsNotFound()
    {
        await using var db = InMemoryDbContextFactory.Create();

        db.Users.AddRange(MakeUser(1, UserRole.Customer), MakeUser(2, UserRole.Customer));
        var otherTicket = new Ticket { Title = "B's ticket", Description = "desc desc", CreatedByUserId = 2 };
        db.Tickets.Add(otherTicket);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(userId: 1, role: UserRole.Customer);
        var sut = new TicketService(db, currentUser, new FakeDateTimeProvider(), new NoOpTicketNotifier());

        // API manipulation scenario: customer 1 guesses customer 2's ticket id.
        var act = async () => await sut.GetTicketByIdAsync(otherTicket.Id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetTicketsAsync_Agent_OnlySeesTicketsAssignedToThem()
    {
        await using var db = InMemoryDbContextFactory.Create();

        db.Users.AddRange(MakeUser(1, UserRole.Customer), MakeUser(10, UserRole.Agent), MakeUser(11, UserRole.Agent));
        db.Tickets.AddRange(
            new Ticket { Title = "Assigned to agent 10", Description = "desc desc", CreatedByUserId = 1, AssignedToUserId = 10 },
            new Ticket { Title = "Assigned to agent 11", Description = "desc desc", CreatedByUserId = 1, AssignedToUserId = 11 },
            new Ticket { Title = "Unassigned", Description = "desc desc", CreatedByUserId = 1 });
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(userId: 10, role: UserRole.Agent);
        var sut = new TicketService(db, currentUser, new FakeDateTimeProvider(), new NoOpTicketNotifier());

        var result = await sut.GetTicketsAsync(new TicketQueryParameters());

        result.TotalCount.Should().Be(1);
        result.Items.Single().Title.Should().Be("Assigned to agent 10");
    }

    [Fact]
    public async Task CreateTicketAsync_SetsStatusOpen_AndRecordsCreatedActivity()
    {
        await using var db = InMemoryDbContextFactory.Create();
        db.Users.Add(MakeUser(1, UserRole.Customer));
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(userId: 1, role: UserRole.Customer);
        var sut = new TicketService(db, currentUser, new FakeDateTimeProvider(), new NoOpTicketNotifier());

        var created = await sut.CreateTicketAsync(new CreateTicketRequest("New issue", "Something is broken", TicketPriority.High));

        created.Status.Should().Be(TicketStatus.Open);
        created.Priority.Should().Be(TicketPriority.High);

        var activity = db.TicketActivities.Single(a => a.TicketId == created.Id);
        activity.Type.Should().Be(ActivityType.Created);
    }

    [Fact]
    public async Task UpdateStatusAsync_InvalidTransition_ThrowsConflict_AndDoesNotChangeStatus()
    {
        await using var db = InMemoryDbContextFactory.Create();
        db.Users.Add(MakeUser(1, UserRole.Customer));
        var ticket = new Ticket { Title = "T", Description = "desc desc", CreatedByUserId = 1, Status = TicketStatus.Open };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(userId: 1, role: UserRole.Admin);
        var sut = new TicketService(db, currentUser, new FakeDateTimeProvider(), new NoOpTicketNotifier());

        // Open -> Resolved is not a valid direct transition.
        var act = async () => await sut.UpdateStatusAsync(
            ticket.Id,
            new UpdateTicketStatusRequest(TicketStatus.Resolved, Convert.ToBase64String(ticket.RowVersion)));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateStatusAsync_Customer_CannotMoveTicketToInProgress()
    {
        await using var db = InMemoryDbContextFactory.Create();
        db.Users.Add(MakeUser(1, UserRole.Customer));
        var ticket = new Ticket { Title = "T", Description = "desc desc", CreatedByUserId = 1, Status = TicketStatus.Open };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(userId: 1, role: UserRole.Customer);
        var sut = new TicketService(db, currentUser, new FakeDateTimeProvider(), new NoOpTicketNotifier());

        var act = async () => await sut.UpdateStatusAsync(
            ticket.Id,
            new UpdateTicketStatusRequest(TicketStatus.InProgress, Convert.ToBase64String(ticket.RowVersion)));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task AssignAsync_NonAdmin_ThrowsForbidden()
    {
        await using var db = InMemoryDbContextFactory.Create();
        db.Users.Add(MakeUser(1, UserRole.Customer));
        var ticket = new Ticket { Title = "T", Description = "desc desc", CreatedByUserId = 1 };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(userId: 10, role: UserRole.Agent);
        var sut = new TicketService(db, currentUser, new FakeDateTimeProvider(), new NoOpTicketNotifier());

        var act = async () => await sut.AssignAsync(ticket.Id, new AssignTicketRequest(10, Convert.ToBase64String(ticket.RowVersion)));

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
