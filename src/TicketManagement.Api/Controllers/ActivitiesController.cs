using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Activities;
using TicketManagement.Application.Activities.Dtos;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets/{ticketId:int}/activities")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketActivityDto>>> GetTimeline(int ticketId, CancellationToken ct)
    {
        return Ok(await _activityService.GetTimelineAsync(ticketId, ct));
    }
}
