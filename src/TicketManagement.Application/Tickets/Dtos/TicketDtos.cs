using System.ComponentModel.DataAnnotations;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Tickets.Dtos;

public record TicketListItemDto(
    int Id,
    string Title,
    TicketStatus Status,
    TicketPriority Priority,
    string CreatedByName,
    string? AssignedToName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    int TotalTimeSpentMinutes);

public record TicketDetailDto(
    int Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    int CreatedByUserId,
    string CreatedByName,
    int? AssignedToUserId,
    string? AssignedToName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? ResolvedAtUtc,
    DateTime? ClosedAtUtc,
    int TotalTimeSpentMinutes,
    string RowVersion);

public record CreateTicketRequest(
    [property: Required, StringLength(200, MinimumLength = 3)] string Title,
    [property: Required, StringLength(4000, MinimumLength = 5)] string Description,
    TicketPriority Priority = TicketPriority.Medium);

public record UpdateTicketDetailsRequest(
    [property: Required, StringLength(200, MinimumLength = 3)] string Title,
    [property: Required, StringLength(4000, MinimumLength = 5)] string Description,
    [property: Required] string RowVersion);

public record UpdateTicketStatusRequest(
    [property: Required] TicketStatus Status,
    [property: Required] string RowVersion);

public record UpdateTicketPriorityRequest(
    [property: Required] TicketPriority Priority,
    [property: Required] string RowVersion);

public record AssignTicketRequest(
    int? AssignedToUserId,
    [property: Required] string RowVersion);

public class TicketQueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is > 0 and <= MaxPageSize ? value : MaxPageSize;
    }

    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? Search { get; set; }

    /// <summary>One of: createdAt, updatedAt, priority, status, title. Prefix with "-" for descending.</summary>
    public string? SortBy { get; set; } = "-createdAt";
}
