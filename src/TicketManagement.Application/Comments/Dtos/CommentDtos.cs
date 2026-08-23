using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Application.Comments.Dtos;

public record CommentDto(int Id, int TicketId, int AuthorUserId, string AuthorName, string Body, DateTime CreatedAtUtc);

public record CreateCommentRequest([property: Required, StringLength(2000, MinimumLength = 1)] string Body);
