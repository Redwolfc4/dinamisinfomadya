using System.Security.Claims;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Mendaftarkan akun pengguna baru.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Login, validasi password, generate JWT, dan pengecekan status Lock Screen.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result.IsLocked)
        {
            return StatusCode(StatusCodes.Status423Locked, result);
        }

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Mengambil informasi pengguna saat ini menggunakan JWT token.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Success = false, Message = "Token tidak valid." });
        }

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
        {
            return NotFound(new { Success = false, Message = "Pengguna tidak ditemukan." });
        }

        return Ok(new { Success = true, Data = user });
    }
}
