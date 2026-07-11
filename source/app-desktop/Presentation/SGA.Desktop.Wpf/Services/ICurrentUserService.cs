using System.Security.Claims;

namespace SGA.Desktop.Wpf.Services;

// Registered as a singleton in the composition root — decodes the JWT's claims once at login and
// exposes them for the rest of the process to read, per app-desktop-wpf/06-identity-and-security.md §4.
public interface ICurrentUserService
{
    ClaimsPrincipal? CurrentUser { get; }
    string? AccessToken { get; }
    string? RefreshToken { get; }
    bool IsAuthenticated { get; }

    event EventHandler? SessionCleared;

    void SetSession(string accessToken, string refreshToken, DateTime accessTokenExpiresAtUtc);
    void ClearSession();
    bool IsInRole(string role);
}
