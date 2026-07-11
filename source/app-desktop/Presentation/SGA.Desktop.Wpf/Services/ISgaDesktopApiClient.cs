using SGA.Desktop.Wpf.Models;

namespace SGA.Desktop.Wpf.Services;

// ViewModels call the API through this client only — never touching a DbContext/repository
// directly, keeping SGA.Desktop.Wpf a genuinely separate BFF layer from SGA.Desktop.Api.
public interface ISgaDesktopApiClient
{
    Task<ApiEnvelope<TokenPairModel>?> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<ApiEnvelope<T>?> GetAsync<T>(string requestUri, CancellationToken ct = default);
    Task<ApiEnvelope<T>?> PostAsync<T>(string requestUri, object? body, CancellationToken ct = default);
    Task<ApiEnvelope?> PostAsync(string requestUri, object? body, CancellationToken ct = default);
    Task<ApiEnvelope<T>?> PutAsync<T>(string requestUri, object? body, CancellationToken ct = default);
}
