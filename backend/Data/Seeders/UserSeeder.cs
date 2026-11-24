using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Seeders;

public static class UserSeeder
{
    /// <summary>
    /// Melakukan seeding akun Administrator ke dalam database (membuat baru atau memperbarui password).
    /// </summary>
    /// <remarks>
    /// Memeriksa apakah user dengan username <c>admin</c> sudah terdaftar. 
    /// Jika belum, user baru akan dibuat. Jika sudah ada, kata sandinya akan diperbarui menggunakan <see cref="PasswordHasher{TUser}"/>.
    /// </remarks>
    /// <param name="context">Instance <see cref="DbContext"/> untuk berinteraksi dengan database.</param>
    /// <param name="password">Kata sandi plaintext yang akan di-hash sebelum disimpan.</param>
    public static void Seed(DbContext context, string password)
    {
        var users = context.Set<User>();
        var hasher = new PasswordHasher<User>();

        var existingUser = users.FirstOrDefault(u => u.Username == "admin");
        if (existingUser != null)
        {
            // Perbarui password jika akun admin sudah ada
            existingUser.Password = hasher.HashPassword(existingUser, password);
            existingUser.UpdatedAt = DateTime.UtcNow;
            context.SaveChanges();
            return;
        }

        var user = new User
        {
            FullName = "Administrator",
            Username = "admin",
            LoginFailed = 0,
            IsLocked = false
        };
        user.Password = hasher.HashPassword(user, password);

        users.Add(user);
        context.SaveChanges();
    }

    /// <summary>
    /// Melakukan seeding akun pengguna demo secara asinkron (membuat baru atau memperbarui password).
    /// </summary>
    /// <remarks>
    /// Memeriksa apakah user dengan username <c>demo</c> sudah terdaftar.
    /// Jika belum, user baru akan dibuat. Jika sudah ada, kata sandinya akan diperbarui.
    /// </remarks>
    /// <param name="context">Instance <see cref="DbContext"/> untuk berinteraksi dengan database.</param>
    /// <param name="password">Kata sandi plaintext yang akan di-hash sebelum disimpan.</param>
    /// <param name="ct">Token pembatalan (<see cref="CancellationToken"/>) untuk membatalkan operasi asinkron.</param>
    /// <returns>Objek <see cref="Task"/> yang merepresentasikan operasi asinkron.</returns>
    public static async Task SeedAsync(
        DbContext context,
        string password,
        CancellationToken ct = default)
    {
        var users = context.Set<User>();
        var hasher = new PasswordHasher<User>();

        var existingUser = await users.FirstOrDefaultAsync(u => u.Username == "demo", ct);
        if (existingUser != null)
        {
            // Perbarui password jika akun demo sudah ada
            existingUser.Password = hasher.HashPassword(existingUser, password);
            existingUser.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
            return;
        }

        var user = new User
        {
            FullName = "Demo User",
            Username = "demo",
            LoginFailed = 0,
            IsLocked = false
        };
        user.Password = hasher.HashPassword(user, password);

        users.Add(user);
        await context.SaveChangesAsync(ct);
    }
}
