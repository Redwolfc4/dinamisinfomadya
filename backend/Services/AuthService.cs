using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Npgsql;

namespace backend.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _hasher = new();

    public const string Issuer = "DinamisInfoMadya";
    public const string Audience = "DinamisInfoMadyaClient";

    public AuthService(
        AppDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // REGISTER
    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request)
    {
        var username = request.Username.Trim().ToLowerInvariant();

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
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? username : request.FullName.Trim(),
            Username = username,
            LoginFailed = 0,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.Password = _hasher.HashPassword(
            user, request.Password
        );

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

    // LOGIN
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request)
    {
        var username = request.Username.Trim().ToLowerInvariant();

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Username == username
            );

        if (user == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Username atau password salah."
            };
        }

        if (user.IsLocked)
        {
            return LockedResponse(user.LoginFailed);
        }

        var result = _hasher.VerifyHashedPassword(
            user,
            user.Password,
            request.Password
        );

        if (result == PasswordVerificationResult.Failed)
        {
            await _context.Users
                .Where(u => u.Id == user.Id && !u.IsLocked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.LoginFailed,
                        u => u.LoginFailed + 1)
                    .SetProperty(u => u.IsLocked,
                        u => u.LoginFailed + 1 > 3)
                    .SetProperty(u => u.UpdatedAt,
                        DateTime.UtcNow));

            user = await _context.Users
                .AsNoTracking()
                .FirstAsync(u => u.Id == user.Id);

            if (user.IsLocked)
            {
                return LockedResponse(user.LoginFailed);
            }

            return new LoginResponse
            {
                Success = false,
                Message = "Username atau password salah.",
                LoginFailedCount = user.LoginFailed,
                IsLocked = false
            };
        }

        // Reset counter jika login berhasil
        var updated = await _context.Users
            .Where(u => u.Id == user.Id && !u.IsLocked)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.LoginFailed, 0)
                .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));

        if (updated == 0)
        {
            return LockedResponse(4);
        }

        return new LoginResponse
        {
            Success = true,
            Message = "Login berhasil.",
            Token = GenerateToken(user),
            LoginFailedCount = 0,
            IsLocked = false
        };
    }

    // GET CURRENT USER (/api/auth/me)
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

    // LOCKED RESPONSE
    private static LoginResponse LockedResponse(int failed)
    {
        return new LoginResponse
        {
            Success = false,
            Message = "Akun terkunci.",
            LoginFailedCount = failed,
            IsLocked = true
        };
    }

    // GENERATE JWT
    private string GenerateToken(User user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key belum dikonfigurasi."
            );

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key)
        );

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),
            new Claim(
                ClaimTypes.Name,
                user.Username
            )
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}