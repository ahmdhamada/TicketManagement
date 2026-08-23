namespace TicketManagement.Domain.Exceptions;

/// <summary>Thrown when an authenticated user tries to access/modify data they do not own or are not permitted to touch.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You are not allowed to perform this action.") : base(message) { }
}
