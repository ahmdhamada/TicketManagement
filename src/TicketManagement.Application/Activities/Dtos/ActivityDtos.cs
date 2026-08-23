using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Activities.Dtos;

public record TicketActivityDto(
    int Id,
    int TicketId,
    int ActorUserId,
    string ActorName,
    ActivityType Type,
    string? OldValue,
    string? NewValue,
    string? Description,
    DateTime CreatedAtUtc);
