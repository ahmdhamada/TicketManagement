using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Dashboard;
using TicketManagement.Application.Dashboard.Dtos;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    private readonly IDashboardService _dashboardService;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(IDashboardService dashboardService, ICacheService cache, ICurrentUserService currentUser)
    {
        _dashboardService = dashboardService;
        _cache = cache;
        _currentUser = currentUser;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken ct)
    {
        // Cache key includes the user id so one role/user's aggregate stats are never
        // served to another (short TTL keeps it acceptably fresh for a dashboard view).
        var cacheKey = $"dashboard-summary-{_currentUser.UserId}";

        if (_cache.TryGet<DashboardSummaryDto>(cacheKey, out var cached) && cached is not null)
        {
            return Ok(cached);
        }

        var summary = await _dashboardService.GetSummaryAsync(ct);
        _cache.Set(cacheKey, summary, CacheDuration);
        return Ok(summary);
    }
}
