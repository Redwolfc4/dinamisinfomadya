using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using frontend.DTOs;

namespace frontend.Services;

public class AuthApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public AuthApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, bool rememberMe = false)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", request);
            var rawContent = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(rawContent))
            {
                var result = JsonSerializer.Deserialize<LoginResponseDto>(
                    rawContent,
                    JsonOptions
                );
                if (result != null) return result;
            }

            return new LoginResponseDto
            {
                Success = false,
                Message = $"Login gagal (HTTP {(int)response.StatusCode})."
            };
        }
        catch (Exception ex)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = $"Tidak dapat terhubung ke server: {ex.Message}"
            };
        }
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", request);
            var rawContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<RegisterResponseDto>(
                    rawContent,
                    JsonOptions
                );
                return result ?? new RegisterResponseDto { Success = true, Message = "Registrasi berhasil." };
            }

            // Handle 400 BadRequest (custom RegisterResponseDto or ASP.NET ValidationProblemDetails)
            if (!string.IsNullOrWhiteSpace(rawContent))
            {
                using var doc = JsonDocument.Parse(rawContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
                {
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = msgProp.GetString() ?? "Registrasi gagal."
                    };
                }

                if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array && prop.Value.GetArrayLength() > 0)
                        {
                            return new RegisterResponseDto
                            {
                                Success = false,
                                Message = prop.Value[0].GetString() ?? "Data registrasi tidak valid."
                            };
                        }
                    }
                }
            }

            return new RegisterResponseDto
            {
                Success = false,
                Message = $"Registrasi gagal (HTTP {(int)response.StatusCode})."
            };
        }
        catch (Exception ex)
        {
            return new RegisterResponseDto
            {
                Success = false,
                Message = $"Tidak dapat terhubung ke server: {ex.Message}"
            };
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
