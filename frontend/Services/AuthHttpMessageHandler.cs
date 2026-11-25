using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using frontend.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace frontend.Services;

public class AuthHttpMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;
    private readonly NavigationManager _navigation;

    public AuthHttpMessageHandler(IJSRuntime js, NavigationManager navigation)
    {
        _js = js;
        _navigation = navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization == null)
        {
            var token = await _js.InvokeAsync<string?>("authCookie.get", "dinamis_auth_token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        var path = request.RequestUri?.AbsolutePath.ToLowerInvariant() ?? "";
        var isAuthEndpoint = path.Contains("/api/auth/login") || path.Contains("/api/auth/refresh");

        if (response.StatusCode == HttpStatusCode.Unauthorized && !isAuthEndpoint)
        {
            var refreshToken = await _js.InvokeAsync<string?>("authCookie.get", "dinamis_refresh_token");
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var refreshed = await TryRefreshTokenAsync(request.RequestUri, refreshToken, cancellationToken);
                if (refreshed != null)
                {
                    await _js.InvokeVoidAsync("authCookie.set", "dinamis_auth_token", refreshed, 1);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed);
                    response.Dispose();
                    return await base.SendAsync(request, cancellationToken);
                }
            }
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            await _js.InvokeVoidAsync("authCookie.remove", "dinamis_auth_token");
            await _js.InvokeVoidAsync("authCookie.remove", "dinamis_refresh_token");

            var currentPath = _navigation.ToBaseRelativePath(_navigation.Uri).TrimEnd('/').ToLowerInvariant();
            if (currentPath != "login" && currentPath != "register" && currentPath != "login/locked")
            {
                _navigation.NavigateTo("/login", replace: true);
            }
        }

        return response;
    }

    private static async Task<string?> TryRefreshTokenAsync(Uri? originalUri, string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var baseUri = originalUri != null
                ? new Uri($"{originalUri.Scheme}://{originalUri.Authority}/")
                : new Uri("http://localhost:5266/");

            using var refreshClient = new HttpClient { BaseAddress = baseUri };
            var refreshResp = await refreshClient.PostAsJsonAsync("api/auth/refresh", new { RefreshToken = refreshToken }, cancellationToken);
            if (!refreshResp.IsSuccessStatusCode) return null;

            var body = await refreshResp.Content.ReadAsStringAsync(cancellationToken);
            var dto = JsonSerializer.Deserialize<LoginResponseDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return dto?.Success == true ? dto.Token : null;
        }
        catch
        {
            return null;
        }
    }
}
