namespace TicketManagement.Domain.Entities;

public class TimeEntry : Common.BaseEntity
{
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateOnly WorkDate { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
}
