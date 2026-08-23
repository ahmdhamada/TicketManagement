namespace TicketManagement.Domain.Enums;

/// <summary>
/// The kind of event recorded on a ticket's activity timeline.
/// </summary>
public enum ActivityType
{
    Created = 1,
    StatusChanged = 2,
    PriorityChanged = 3,
    AssigneeChanged = 4,
    CommentAdded = 5,
    TimeLogged = 6
}
