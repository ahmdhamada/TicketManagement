using Microsoft.EntityFrameworkCore;
using TicketManagement.Infrastructure.Persistence;

namespace TicketManagement.UnitTests.Application.TestDoubles;

public static class InMemoryDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
