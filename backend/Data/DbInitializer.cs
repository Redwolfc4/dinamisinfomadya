using backend.Data.Seeders;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static partial class DbInitializer
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Memeriksa dan menerapkan migrasi database...")]
    private static partial void LogCheckingMigrations(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Menjalankan seeder data pengguna awal...")]
    private static partial void LogSeedingUsers(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Inisialisasi database dan seeder berhasil diselesaikan!")]
    private static partial void LogInitSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Terjadi kesalahan saat melakukan migrasi atau seeding database.")]
    private static partial void LogInitError(ILogger logger, Exception ex);

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
                LogCheckingMigrations(logger);
                await context.Database.MigrateAsync();
            }

            LogSeedingUsers(logger);

            await UserSeeder.SeedAsync(context, CancellationToken.None);

            LogInitSuccess(logger);
        }
        catch (Exception ex)
        {
            LogInitError(logger, ex);
            throw;
        }
    }
}
