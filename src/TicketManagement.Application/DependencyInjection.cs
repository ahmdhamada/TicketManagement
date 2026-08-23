using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TicketManagement.Application.Activities;
using TicketManagement.Application.Auth;
using TicketManagement.Application.Comments;
using TicketManagement.Application.Dashboard;
using TicketManagement.Application.TimeEntries;
using TicketManagement.Application.Tickets;
using TicketManagement.Application.Users;

namespace TicketManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ITimeEntryService, TimeEntryService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
