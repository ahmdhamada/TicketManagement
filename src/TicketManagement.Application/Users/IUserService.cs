using TicketManagement.Application.Users.Dtos;

namespace TicketManagement.Application.Users;

/// <summary>Admin-only user management (list, create, update role/active state).</summary>
public interface IUserService
{
    Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken ct = default);
    Task<AdminUserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<AdminUserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<AdminUserDto>> GetAgentsAsync(CancellationToken ct = default);
}
