using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Users.Dtos;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Application.Users;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IApplicationDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Users.AsNoTracking().OrderBy(u => u.FullName).Select(u => ToDto(u)).ToListAsync(ct);

    public async Task<IReadOnlyList<AdminUserDto>> GetAgentsAsync(CancellationToken ct = default) =>
        await _db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.Agent && u.IsActive)
            .OrderBy(u => u.FullName)
            .Select(u => ToDto(u))
            .ToListAsync(ct);

    public async Task<AdminUserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct))
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task<AdminUserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync(new object?[] { id }, ct) ?? throw new NotFoundException(nameof(User), id);

        user.FullName = request.FullName.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    private static AdminUserDto ToDto(User u) => new(u.Id, u.FullName, u.Email, u.Role, u.IsActive, u.CreatedAtUtc);
}
