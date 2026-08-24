using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Application.TimeEntries.Dtos;

public record TimeEntryDto(int Id, int TicketId, int UserId, string UserName, DateOnly WorkDate, int DurationMinutes, string? Description, DateTime CreatedAtUtc);

public record CreateTimeEntryRequest(
    [ Required] DateOnly WorkDate,
    [ Range(1, 24 * 60)] int DurationMinutes,
    [ StringLength(1000)] string? Description);

public record TicketTimeSummaryDto(int TicketId, int TotalMinutes, IReadOnlyList<TimeEntryDto> Entries);
