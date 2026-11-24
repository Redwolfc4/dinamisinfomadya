using System.Text.RegularExpressions;
using backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==============================================================================
// 1. DYNAMIC DOTENV LOADER (.env & .env.development)
// ==============================================================================
void LoadEnvFile(string fileName)
{
    var searchPaths = new[]
    {
        Path.Combine(builder.Environment.ContentRootPath, fileName),
        Path.Combine(Directory.GetCurrentDirectory(), fileName),
        Path.Combine(Directory.GetCurrentDirectory(), "backend", fileName),
        Path.Combine(AppContext.BaseDirectory, fileName)
    };

    var filePath = searchPaths.FirstOrDefault(File.Exists);
    if (filePath == null) return;

    foreach (var line in File.ReadAllLines(filePath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            continue;

        var parts = trimmed.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}

// Muat .env default terlebih dahulu
LoadEnvFile(".env");
// Muat dan override dengan .env.development jika berada di lingkungan Development
LoadEnvFile($".env.{builder.Environment.EnvironmentName.ToLower()}");

// ==============================================================================
// 2. RESOLVE ENVIRONMENT TOKENS (${KEY}) PADA KONFIGURASI
// ==============================================================================
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(rawConnectionString))
{
    // Mengganti token seperti ${DATABASE_URL} atau ${DB_HOST} dengan nilai dari .env
    var resolved = Regex.Replace(
        rawConnectionString,
        @"\$\{([A-Za-z0-9_]+)\}",
        match => Environment.GetEnvironmentVariable(match.Groups[1].Value)
                 ?? Environment.GetEnvironmentVariable(match.Groups[1].Value.ToUpperInvariant())
                 ?? match.Value
    );

    // Update nilai hasil injeksi kembali ke builder.Configuration
    builder.Configuration["ConnectionStrings:DefaultConnection"] = resolved;
}

// ==============================================================================
// 3. DEPENDENCY INJECTION CONTAINER
// ==============================================================================
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Dapatkan Connection String yang telah diinjeksi dan di-resolve
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("${"))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' belum terkonfigurasi dengan benar. Pastikan file .env atau .env.development sudah memuat variabel yang dibutuhkan.");
}

// Daftarkan AppDbContext dengan provider PostgreSQL (Npgsql)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// ==============================================================================
// 4. CLI SEEDER EXECUTION
// ==============================================================================
// Jika dijalankan dengan parameter '--seed', eksekusi migrasi & seeder lalu selesai
if (args.Contains("--seed"))
{
    await app.InitializeDatabaseAsync();
    return;
}

// ==============================================================================
// 5. HTTP REQUEST PIPELINE
// ==============================================================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
