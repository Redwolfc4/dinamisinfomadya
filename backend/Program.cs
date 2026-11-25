using System.Text;
using System.Text.RegularExpressions;
using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

if (builder.Environment.IsDevelopment())
{
    LoadEnvFile(".env.development");
}
else
{
    LoadEnvFile(".env");
    LoadEnvFile(".env.production");
}

// ==============================================================================
// 2. RESOLVE ENVIRONMENT TOKENS (${KEY}) PADA KONFIGURASI
// ==============================================================================
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(rawConnectionString))
{
    var resolved = Regex.Replace(
        rawConnectionString,
        @"\$\{([A-Za-z0-9_]+)\}",
        match => Environment.GetEnvironmentVariable(match.Groups[1].Value)
                 ?? Environment.GetEnvironmentVariable(match.Groups[1].Value.ToUpperInvariant())
                 ?? match.Value
    );

    builder.Configuration["ConnectionStrings:DefaultConnection"] = resolved;
}

// ==============================================================================
// 3. DEPENDENCY INJECTION CONTAINER
// ==============================================================================
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<RegisterService>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5181", "https://localhost:7180")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key belum dikonfigurasi di User Secrets atau environment.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = UserService.Issuer,
            ValidAudience = UserService.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("${"))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' belum terkonfigurasi dengan benar. Pastikan file .env atau .env.development sudah memuat variabel yang dibutuhkan.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// ==============================================================================
// 4. CLI SEEDER EXECUTION
// ==============================================================================
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
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
