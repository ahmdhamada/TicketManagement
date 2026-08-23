using System.Net;
using System.Text.Json;
using FluentValidation;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Api.Middleware;

/// <summary>
/// Centralized exception handler: every unhandled exception is translated into a
/// consistent ProblemDetails-shaped JSON body and logged once, instead of letting
/// each controller action catch-and-format its own errors.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException vex => (
                HttpStatusCode.BadRequest,
                "One or more validation errors occurred.",
                vex.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
            ForbiddenException => (HttpStatusCode.Forbidden, exception.Message, null),
            ConflictException => (HttpStatusCode.Conflict, exception.Message, null),
            DomainException => (HttpStatusCode.BadRequest, exception.Message, null),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message, null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception ({StatusCode}) processing {Method} {Path}", (int)statusCode, context.Request.Method, context.Request.Path);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new
        {
            type = $"https://httpstatuses.io/{(int)statusCode}",
            title,
            status = (int)statusCode,
            errors,
            traceId = context.TraceIdentifier,
            detail = _env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError ? exception.ToString() : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }));
    }
}
