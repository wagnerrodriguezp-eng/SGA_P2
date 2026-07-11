using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SGA.Desktop.Wpf.Models;

namespace SGA.Desktop.Wpf.Services;

public class SgaDesktopApiClient : ISgaDesktopApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ICurrentUserService _currentUser;

    public SgaDesktopApiClient(HttpClient httpClient, ICurrentUserService currentUser)
    {
        _httpClient = httpClient;
        _currentUser = currentUser;
    }

    public async Task<ApiEnvelope<TokenPairModel>?> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", new { email, password }, JsonOptions, ct);
            return await ReadEnvelopeAsync<TokenPairModel>(response, ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<ApiEnvelope<T>?> GetAsync<T>(string requestUri, CancellationToken ct = default)
    {
        var response = await SendAsync(() => new HttpRequestMessage(HttpMethod.Get, requestUri), ct);
        return response is null ? null : await ReadEnvelopeAsync<T>(response, ct);
    }

    public async Task<ApiEnvelope<T>?> PostAsync<T>(string requestUri, object? body, CancellationToken ct = default)
    {
        var response = await SendAsync(() => CreateJsonRequest(HttpMethod.Post, requestUri, body), ct);
        return response is null ? null : await ReadEnvelopeAsync<T>(response, ct);
    }

    public async Task<ApiEnvelope?> PostAsync(string requestUri, object? body, CancellationToken ct = default)
    {
        var response = await SendAsync(() => CreateJsonRequest(HttpMethod.Post, requestUri, body), ct);
        if (response is null)
        {
            return null;
        }
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiEnvelope>(JsonOptions, ct);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<ApiEnvelope<T>?> PutAsync<T>(string requestUri, object? body, CancellationToken ct = default)
    {
        var response = await SendAsync(() => CreateJsonRequest(HttpMethod.Put, requestUri, body), ct);
        return response is null ? null : await ReadEnvelopeAsync<T>(response, ct);
    }

    // Attaches the bearer token, sends the request, and on a 401 attempts exactly one silent
    // refresh-token exchange before retrying once — per app-desktop-wpf/06 §4. If refresh also
    // fails, clears the session so the shell can navigate back to the login view.
    private async Task<HttpResponseMessage?> SendAsync(Func<HttpRequestMessage> requestFactory, CancellationToken ct)
    {
        AttachToken();
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(requestFactory(), ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        if (!await TryRefreshAsync(ct))
        {
            _currentUser.ClearSession();
            return response;
        }

        AttachToken();
        try
        {
            return await _httpClient.SendAsync(requestFactory(), ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private async Task<bool> TryRefreshAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_currentUser.RefreshToken))
        {
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/auth/refresh", new { refreshToken = _currentUser.RefreshToken }, JsonOptions, ct);
            var envelope = await ReadEnvelopeAsync<TokenPairModel>(response, ct);
            if (envelope is not { IsSuccess: true, Data: not null })
            {
                return false;
            }

            _currentUser.SetSession(envelope.Data.AccessToken, envelope.Data.RefreshToken, envelope.Data.AccessTokenExpiresAtUtc);
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    private void AttachToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            _currentUser.AccessToken is { Length: > 0 } token ? new AuthenticationHeaderValue("Bearer", token) : null;
    }

    private static HttpRequestMessage CreateJsonRequest(HttpMethod method, string requestUri, object? body)
    {
        var request = new HttpRequestMessage(method, requestUri);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }
        return request;
    }

    private static async Task<ApiEnvelope<T>?> ReadEnvelopeAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>(JsonOptions, ct);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
