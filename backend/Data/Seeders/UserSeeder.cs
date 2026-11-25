using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Seeders;

public static class UserSeeder
{
    // =========================================================================
    // DATA SEEDER USER
    // =========================================================================
    private static readonly (string Username, string FullName, string Password)[] SeedAccounts =
    [
        ("admin", "Administrator", "dinamisinfomadya123#"),
        ("demo", "Demo User", "dinamisinfomadya123#")
    ];

    /// <summary>
    /// Melakukan seeding akun Administrator dan Demo User ke dalam database secara asinkron (membuat baru atau memperbarui password).
    /// </summary>
    /// <param name="context">Instance <see cref="DbContext"/> untuk berinteraksi dengan database.</param>
    /// <param name="ct">Token pembatalan (<see cref="CancellationToken"/>) untuk membatalkan operasi asinkron.</param>
    public static async Task SeedAsync(
        DbContext context,
        CancellationToken ct = default)
    {
        var users = context.Set<User>();
        var hasher = new PasswordHasher<User>();

        foreach (var (username, fullName, password) in SeedAccounts)
        {
            var existingUser = await users.FirstOrDefaultAsync(u => u.Username == username, ct);
            if (existingUser != null)
            {
                existingUser.Password = hasher.HashPassword(existingUser, password);
                existingUser.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var user = new User
                {
                    FullName = fullName,
                    Username = username,
                    LoginFailed = 0,
                    IsLocked = false
                };
                user.Password = hasher.HashPassword(user, password);
                users.Add(user);
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
