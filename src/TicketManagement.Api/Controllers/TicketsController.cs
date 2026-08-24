using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Tickets;
using TicketManagement.Application.Tickets.Dtos;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTickets([FromQuery] TicketQueryParameters query, CancellationToken ct)
    {
        var result = await _ticketService.GetTicketsAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDetailDto>> GetTicket(int id, CancellationToken ct)
    {
        return Ok(await _ticketService.GetTicketByIdAsync(id, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<TicketDetailDto>> CreateTicket(CreateTicketRequest request, CancellationToken ct)
    {
        var ticket = await _ticketService.CreateTicketAsync(request, ct);
        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TicketDetailDto>> UpdateDetails(int id, UpdateTicketDetailsRequest request, CancellationToken ct)
    {
        return Ok(await _ticketService.UpdateDetailsAsync(id, request, ct));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TicketDetailDto>> UpdateStatus(int id, UpdateTicketStatusRequest request, CancellationToken ct)
    {
        return Ok(await _ticketService.UpdateStatusAsync(id, request, ct));
    }

    [HttpPatch("{id:int}/priority")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<ActionResult<TicketDetailDto>> UpdatePriority(int id, UpdateTicketPriorityRequest request, CancellationToken ct)
    {
        return Ok(await _ticketService.UpdatePriorityAsync(id, request, ct));
    }

    [HttpPatch("{id:int}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TicketDetailDto>> Assign(int id, AssignTicketRequest request, CancellationToken ct)
    {
        return Ok(await _ticketService.AssignAsync(id, request, ct));
    }
}
