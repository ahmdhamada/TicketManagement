using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Users;
using TicketManagement.Application.Users.Dtos;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetAll(CancellationToken ct) => Ok(await _userService.GetAllAsync(ct));

    [HttpGet("agents")]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetAgents(CancellationToken ct) => Ok(await _userService.GetAgentsAsync(ct));

    [HttpPost]
    public async Task<ActionResult<AdminUserDto>> Create(CreateUserRequest request, CancellationToken ct)
    {
        var user = await _userService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), user);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminUserDto>> Update(int id, UpdateUserRequest request, CancellationToken ct) =>
        Ok(await _userService.UpdateAsync(id, request, ct));
}
