using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UserService
{
    public const string Issuer = "DinamisInfoMadya";
    public const string Audience = "DinamisInfoMadyaClient";

    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object?> GetCurrentUserAsync(Guid userId)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Username,
                u.IsLocked,
                u.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Username = u.Username,
                LoginFailed = u.LoginFailed,
                IsLocked = u.IsLocked,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, UserDto? Data)> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request,
        Guid currentUserId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return (false, "Pengguna tidak ditemukan.", null);
        }

        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var duplicate = await _context.Users
            .AnyAsync(u => u.Username == normalizedUsername && u.Id != id);

        if (duplicate)
        {
            return (false, "Username sudah digunakan oleh pengguna lain.", null);
        }

        user.FullName = request.FullName.Trim();
        user.Username = normalizedUsername;
        user.IsLocked = request.IsLocked;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, request.Password);
        }

        // Jika lock dimatikan secara manual, reset LoginFailed ke 0
        if (!request.IsLocked)
        {
            user.LoginFailed = 0;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Length < 8)
            {
                return (false, "Password baru minimal 8 karakter.", null);
            }

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<backend.Models.User>();
            user.Password = hasher.HashPassword(user, request.Password);
        }

        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;

        await _context.SaveChangesAsync();

        return (true, "Data pengguna berhasil diperbarui.", new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            LoginFailed = user.LoginFailed,
            IsLocked = user.IsLocked,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        });
    }

    public async Task<(bool Success, string Message)> SoftDeleteUserAsync(Guid id, Guid currentUserId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return (false, "Pengguna tidak ditemukan.");
        }

        var now = DateTime.UtcNow;
        user.DeletedAt = now;
        user.DeletedBy = currentUserId;
        user.UpdatedAt = now;
        user.UpdatedBy = currentUserId;

        await _context.SaveChangesAsync();

        return (true, "Pengguna berhasil dihapus (soft delete).");
    }
}
