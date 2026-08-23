using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Infrastructure.Persistence;
using TicketManagement.Infrastructure.Services;

namespace TicketManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        return services;
    }
}
