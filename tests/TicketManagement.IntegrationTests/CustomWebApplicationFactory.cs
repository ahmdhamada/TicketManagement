using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Infrastructure.Persistence;
using TicketManagement.Infrastructure.Persistence.Seed;

namespace TicketManagement.IntegrationTests;

/// <summary>
/// Boots the real API pipeline (auth, middleware, controllers) against a private
/// SQLite in-memory database so integration tests exercise actual HTTP + EF Core
/// behavior without requiring a SQL Server instance.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            DbInitializer.SeedAsync(db, passwordHasher).GetAwaiter().GetResult();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
