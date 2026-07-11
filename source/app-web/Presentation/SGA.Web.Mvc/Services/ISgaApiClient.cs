using SGA.Web.Mvc.Models.Api;

namespace SGA.Web.Mvc.Services;

// MVC controllers call the API through this client only — never touching a DbContext/repository
// directly — keeping SGA.Web.Mvc a genuinely separate BFF layer from SGA.Web.Api.
public interface ISgaApiClient
{
    Task<ApiEnvelope<T>?> GetAsync<T>(string requestUri, CancellationToken ct = default);
    Task<ApiEnvelope<T>?> PostAsync<T>(string requestUri, object? body, CancellationToken ct = default);
    Task<ApiEnvelope?> PostAsync(string requestUri, object? body, CancellationToken ct = default);
}
