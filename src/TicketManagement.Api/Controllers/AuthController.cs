using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TicketManagement.Application.Auth;
using TicketManagement.Application.Auth.Dtos;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ClientIp, ct);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken, ClientIp, ct);
        return Ok(result);
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(RevokeTokenRequest request, CancellationToken ct)
    {
        await _authService.RevokeAsync(request.RefreshToken, ClientIp, ct);
        return NoContent();
    }
}
