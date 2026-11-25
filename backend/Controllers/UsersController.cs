using System.Security.Claims;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(new { Success = true, Data = users });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized(new { Success = false, Message = "Token tidak valid." });
        }

        var (success, message, data) = await _userService.UpdateUserAsync(id, request, currentUserId);
        if (!success)
        {
            return BadRequest(new { Success = false, Message = message });
        }

        return Ok(new { Success = true, Message = message, Data = data });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized(new { Success = false, Message = "Token tidak valid." });
        }

        var (success, message) = await _userService.SoftDeleteUserAsync(id, currentUserId);
        if (!success)
        {
            return NotFound(new { Success = false, Message = message });
        }

        return Ok(new { Success = true, Message = message });
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }
}

