using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.TimeEntries;
using TicketManagement.Application.TimeEntries.Dtos;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets/{ticketId:int}/time-entries")]
public class TimeEntriesController : ControllerBase
{
    private readonly ITimeEntryService _timeEntryService;

    public TimeEntriesController(ITimeEntryService timeEntryService)
    {
        _timeEntryService = timeEntryService;
    }

    [HttpGet]
    public async Task<ActionResult<TicketTimeSummaryDto>> GetTimeEntries(int ticketId, CancellationToken ct)
    {
        return Ok(await _timeEntryService.GetForTicketAsync(ticketId, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<ActionResult<TimeEntryDto>> LogTime(int ticketId, CreateTimeEntryRequest request, CancellationToken ct)
    {
        var entry = await _timeEntryService.LogTimeAsync(ticketId, request, ct);
        return CreatedAtAction(nameof(GetTimeEntries), new { ticketId }, entry);
    }
}
