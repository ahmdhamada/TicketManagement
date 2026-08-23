using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Rules;

/// <summary>
/// Centralizes the ticket status state machine so the same rule is enforced
/// everywhere (API, services, tests) instead of being duplicated ad hoc.
///
/// Allowed transitions:
///   Open        -> InProgress, Closed
///   InProgress  -> Resolved, Open, Closed
///   Resolved    -> Closed, InProgress (reopened)
///   Closed      -> (terminal; no further transitions)
/// </summary>
public static class TicketStatusRules
{
    private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedTransitions = new()
    {
        [TicketStatus.Open] = new[] { TicketStatus.InProgress, TicketStatus.Closed },
        [TicketStatus.InProgress] = new[] { TicketStatus.Resolved, TicketStatus.Open, TicketStatus.Closed },
        [TicketStatus.Resolved] = new[] { TicketStatus.Closed, TicketStatus.InProgress },
        [TicketStatus.Closed] = Array.Empty<TicketStatus>()
    };

    public static bool CanTransition(TicketStatus from, TicketStatus to)
    {
        if (from == to)
        {
            return true;
        }

        return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    /// <summary>
    /// Customers are only allowed to close a ticket that has already been resolved,
    /// and only support staff (Admin/Agent) can move a ticket into any other status.
    /// </summary>
    public static bool CanCustomerTransition(TicketStatus from, TicketStatus to)
    {
        if (from == to)
        {
            return true;
        }

        return from == TicketStatus.Resolved && to == TicketStatus.Closed;
    }
}
