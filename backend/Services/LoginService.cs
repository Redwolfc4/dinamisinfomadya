using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public class LoginService
{
    public const int MaxLoginAttempts = 3;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _hasher = new();

    public LoginService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var (username, password) = request;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Username atau password salah."
            };
        }

        var result = _hasher.VerifyHashedPassword(user, user.Password, password);

        if (result == PasswordVerificationResult.Failed || user.IsLocked)
        {
            await _context.Users
                .Where(u => u.Id == user.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.LoginFailed, u => u.LoginFailed + 1)
                    .SetProperty(u => u.IsLocked, u => u.LoginFailed + 1 >= MaxLoginAttempts)
                    .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));

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
                MaxLoginAttempts = MaxLoginAttempts,
                IsLocked = false
            };
        }

        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Reset counter & simpan refresh token (7 hari) jika login berhasil
        var updated = await _context.Users
            .Where(u => u.Id == user.Id && !u.IsLocked)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.LoginFailed, 0)
                .SetProperty(u => u.RefreshToken, refreshToken)
                .SetProperty(u => u.RefreshTokenExpiresAt, refreshTokenExpiresAt)
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
            RefreshToken = refreshToken,
            LoginFailedCount = 0,
            IsLocked = false
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return new LoginResponse { Success = false, Message = "Refresh token tidak valid." };
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.IsLocked || user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Sesi telah berakhir. Silakan login kembali."
            };
        }

        return new LoginResponse
        {
            Success = true,
            Message = "Token berhasil diperbarui.",
            Token = GenerateToken(user),
            RefreshToken = user.RefreshToken,
            LoginFailedCount = 0,
            IsLocked = false
        };
    }

    public async Task LogoutAsync(Guid userId)
    {
        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.RefreshToken, (string?)null)
                .SetProperty(u => u.RefreshTokenExpiresAt, (DateTime?)null)
                .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));
    }

    private static LoginResponse LockedResponse(int failed)
    {
        return new LoginResponse
        {
            Success = false,
            Message = "Akun Anda telah dikunci. Silakan hubungi administrator untuk membuka kembali.",
            LoginFailedCount = failed,
            MaxLoginAttempts = MaxLoginAttempts,
            IsLocked = true
        };
    }

    private string GenerateToken(User user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key belum dikonfigurasi.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: UserService.Issuer,
            audience: UserService.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}

