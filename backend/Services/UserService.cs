using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public const string Issuer = "DinamisInfoMadya";
    public const string Audience = "DinamisInfoMadyaClient";

    public UserService(AppDbContext context)
    {
        _context = context;
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
}
