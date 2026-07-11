using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SGA.Web.Mvc.Models.Api;

namespace SGA.Web.Mvc.Services;

public class SgaApiClient : ISgaApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SgaApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiEnvelope<T>?> GetAsync<T>(string requestUri, CancellationToken ct = default)
    {
        AttachToken();
        try
        {
            var response = await _httpClient.GetAsync(requestUri, ct);
            return await ReadEnvelopeAsync<T>(response, ct);
        }
        catch (HttpRequestException)
        {
            return null; // signals "service unavailable" to the caller
        }
    }

    public async Task<ApiEnvelope<T>?> PostAsync<T>(string requestUri, object? body, CancellationToken ct = default)
    {
        AttachToken();
        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, body, JsonOptions, ct);
            return await ReadEnvelopeAsync<T>(response, ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<ApiEnvelope?> PostAsync(string requestUri, object? body, CancellationToken ct = default)
    {
        AttachToken();
        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, body, JsonOptions, ct);
            return await response.Content.ReadFromJsonAsync<ApiEnvelope>(JsonOptions, ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private void AttachToken()
    {
        var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
        _httpClient.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
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
