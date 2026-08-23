using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Comments;
using TicketManagement.Application.Comments.Dtos;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets/{ticketId:int}/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> GetComments(int ticketId, CancellationToken ct)
    {
        return Ok(await _commentService.GetCommentsForTicketAsync(ticketId, ct));
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment(int ticketId, CreateCommentRequest request, CancellationToken ct)
    {
        var comment = await _commentService.AddCommentAsync(ticketId, request, ct);
        return CreatedAtAction(nameof(GetComments), new { ticketId }, comment);
    }
}
