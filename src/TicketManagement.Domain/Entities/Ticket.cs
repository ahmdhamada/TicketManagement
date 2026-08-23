using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Entities;

public class Ticket : Common.BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }

    /// <summary>Optimistic-concurrency token (SQL Server ROWVERSION).</summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TicketActivity> Activities { get; set; } = new List<TicketActivity>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    public int TotalTimeSpentMinutes => TimeEntries?.Sum(t => t.DurationMinutes) ?? 0;
}
