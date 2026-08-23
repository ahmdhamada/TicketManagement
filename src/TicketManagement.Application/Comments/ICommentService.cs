using TicketManagement.Application.Comments.Dtos;

namespace TicketManagement.Application.Comments;

public interface ICommentService
{
    Task<IReadOnlyList<CommentDto>> GetCommentsForTicketAsync(int ticketId, CancellationToken ct = default);
    Task<CommentDto> AddCommentAsync(int ticketId, CreateCommentRequest request, CancellationToken ct = default);
}
