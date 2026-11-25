using backend.Data.Seeders;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class DbInitializer
{
    /// <summary>
    /// Menjalankan migrasi database otomatis dan proses seeding akun awal.
    /// </summary>
    /// <param name="app">Instance WebApplication yang sedang berjalan.</param>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            var context = services.GetRequiredService<AppDbContext>();

            // Menerapkan pending migrations jika koneksi database tersedia
            if (context.Database.IsRelational())
            {
                logger.LogInformation("Memeriksa dan menerapkan migrasi database...");
                await context.Database.MigrateAsync();
            }

            logger.LogInformation("Menjalankan seeder data pengguna awal...");

            await UserSeeder.SeedAsync(context, CancellationToken.None);

            logger.LogInformation("Inisialisasi database dan seeder berhasil diselesaikan!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Terjadi kesalahan saat melakukan migrasi atau seeding database.");
            throw;
        }
    }
}
