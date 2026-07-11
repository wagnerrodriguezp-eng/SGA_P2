using SGA.SharedKernel.Application.Results;

namespace SGA.Desktop.Application.Identity;

public record TokenPairResult(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken);

public record RegisteredAccount(Guid UserId, string EmailConfirmationToken);

// Facade over SGA.Identity's account/token services plus this app's own refresh-token store,
// implemented in the Persistence project (which references SGA.Identity) so the Application layer
// never references Identity directly.
public interface IIdentityGateway
{
    Task<OperationResult<TokenPairResult>> LoginAsync(string email, string password, CancellationToken ct = default);

    Task<OperationResult<TokenPairResult>> RefreshAsync(string refreshToken, CancellationToken ct = default);

    Task<OperationResult<RegisteredAccount>> RegisterAccountAsync(
        string email, string password, string firstName, string lastName, string roleName, CancellationToken ct = default);

    // Resolves emails for a set of users — used by the trip-cancellation notification fan-out,
    // which otherwise has no way to reach ApplicationUser data from the Application layer.
    Task<IReadOnlyDictionary<Guid, string>> GetUserEmailsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
}
