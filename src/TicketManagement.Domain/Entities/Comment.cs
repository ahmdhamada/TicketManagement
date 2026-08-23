namespace TicketManagement.Domain.Entities;

public class Comment : Common.BaseEntity
{
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int AuthorUserId { get; set; }
    public User AuthorUser { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
}
