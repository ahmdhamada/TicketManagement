using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Entities;

public class User : Common.BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
