using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SGA.Desktop.Wpf.Services;

public class CurrentUserService : ICurrentUserService
{
    public ClaimsPrincipal? CurrentUser { get; private set; }
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public bool IsAuthenticated => CurrentUser?.Identity?.IsAuthenticated == true;

    public event EventHandler? SessionCleared;

    public void SetSession(string accessToken, string refreshToken, DateTime accessTokenExpiresAtUtc)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;

        // ReadJwtToken does not apply inbound claim-type mapping, so claim types keep their raw
        // JWT names here (e.g. "email" rather than the long ClaimTypes.Email URI).
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var identity = new ClaimsIdentity(jwt.Claims, "Bearer", JwtRegisteredClaimNames.Email, ClaimTypes.Role);
        CurrentUser = new ClaimsPrincipal(identity);
    }

    public void ClearSession()
    {
        AccessToken = null;
        RefreshToken = null;
        CurrentUser = null;
        SessionCleared?.Invoke(this, EventArgs.Empty);
    }

    public bool IsInRole(string role) => CurrentUser?.IsInRole(role) == true;
}
