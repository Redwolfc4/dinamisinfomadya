using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using frontend.DTOs;
using Microsoft.JSInterop;

namespace frontend.Services;

public class UserApiService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public UserApiService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    private async Task AttachTokenAsync()
    {
        var token = await _js.InvokeAsync<string?>("authCookie.get", "dinamis_auth_token");
        if (!string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<(bool Success, string Message, List<UserDto> Data, int TotalCount, int TotalPages)> GetUsersPagedAsync(
        string? search = null,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            await AttachTokenAsync();
            var url = $"api/users?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search.Trim())}";
            }

            var response = await _http.GetAsync(url);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return (false, $"Gagal memuat data user (HTTP {(int)response.StatusCode}).", [], 0, 1);
            }

            var parsed = JsonSerializer.Deserialize<PagedUsersApiResult>(raw, JsonOptions);
            return (true, "OK", parsed?.Data ?? [], parsed?.TotalCount ?? 0, Math.Max(1, parsed?.TotalPages ?? 1));
        }
        catch (Exception ex)
        {
            return (false, $"Tidak dapat terhubung ke server: {ex.Message}", [], 0, 1);
        }
    }

    private class PagedUsersApiResult
    {
        public bool Success { get; set; }
        public List<UserDto>? Data { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public async Task<(bool Success, string Message, List<UserDto> Data)> GetUsersAsync()
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.GetAsync("api/users");
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return (false, $"Gagal memuat data user (HTTP {(int)response.StatusCode}).", []);
            }

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("data", out var dataProp))
            {
                var users = JsonSerializer.Deserialize<List<UserDto>>(dataProp.GetRawText(), JsonOptions) ?? [];
                return (true, string.Empty, users);
            }

            return (true, string.Empty, []);
        }
        catch (Exception ex)
        {
            return (false, $"Tidak dapat terhubung ke server: {ex.Message}", []);
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PutAsJsonAsync($"api/users/{id}", request);
            var raw = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(raw))
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("message", out var msgProp))
                {
                    return (response.IsSuccessStatusCode, msgProp.GetString() ?? string.Empty);
                }
            }

            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Berhasil diperbarui." : $"Gagal memperbarui (HTTP {(int)response.StatusCode}).");
        }
        catch (Exception ex)
        {
            return (false, $"Tidak dapat terhubung ke server: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteUserAsync(Guid id)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.DeleteAsync($"api/users/{id}");
            var raw = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(raw))
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("message", out var msgProp))
                {
                    return (response.IsSuccessStatusCode, msgProp.GetString() ?? string.Empty);
                }
            }

            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Pengguna berhasil dihapus." : $"Gagal menghapus (HTTP {(int)response.StatusCode}).");
        }
        catch (Exception ex)
        {
            return (false, $"Tidak dapat terhubung ke server: {ex.Message}");
        }
    }
}

