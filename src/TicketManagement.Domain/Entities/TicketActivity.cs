using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// An immutable timeline entry describing something that happened to a ticket.
/// </summary>
public class TicketActivity : Common.BaseEntity
{
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int ActorUserId { get; set; }
    public User ActorUser { get; set; } = null!;

    public ActivityType Type { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Description { get; set; }
}
