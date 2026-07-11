using SGA.Identity.Entities;

namespace SGA.Identity.Services;

public record AccessToken(string Token, DateTime ExpiresAtUtc, Guid UserId);

public interface ITokenService
{
    AccessToken CreateAccessToken(ApplicationUser user, IReadOnlyCollection<string> roles);
}
