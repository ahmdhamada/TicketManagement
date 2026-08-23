using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Common;

/// <summary>
/// The single source of truth for role-based ticket visibility, shared by every
/// service that touches tickets (tickets, comments, time entries) so the customer
/// data-isolation rule can never drift between call sites.
/// </summary>
public static class TicketVisibilityExtensions
{
    public static IQueryable<Ticket> VisibleTo(this IQueryable<Ticket> query, ICurrentUserService currentUser)
    {
        return currentUser.Role switch
        {
            UserRole.Admin => query,
            UserRole.Agent => query.Where(t => t.AssignedToUserId == currentUser.UserId),
            UserRole.Customer => query.Where(t => t.CreatedByUserId == currentUser.UserId),
            _ => query.Where(_ => false)
        };
    }
}
