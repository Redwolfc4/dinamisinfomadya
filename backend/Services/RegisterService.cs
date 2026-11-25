using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Services;

public class RegisterService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _hasher = new();

    public RegisterService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {

        var (fullName, username, password) = request;

        var exists = await _context.Users
            .AnyAsync(u => u.Username == username);

        if (exists)
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "Username sudah terdaftar."
            };
        }

        var user = new User
        {
            FullName = fullName,
            Username = username,
            LoginFailed = 0,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.Password = _hasher.HashPassword(user, request.Password);

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg
                && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "Username sudah terdaftar."
            };
        }

        return new RegisterResponse
        {
            Success = true,
            Message = "Registrasi berhasil."
        };
    }
}

