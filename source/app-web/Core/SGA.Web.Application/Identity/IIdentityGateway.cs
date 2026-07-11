using SGA.SharedKernel.Application.Results;

namespace SGA.Web.Application.Identity;

public record TokenResult(string Token, DateTime ExpiresAtUtc);

public record RegisteredAccount(Guid UserId, string EmailConfirmationToken);

public record PasswordResetToken(Guid UserId, string Token);

// Facade over SGA.Identity's account/token services, implemented in the Persistence project
// (which references SGA.Identity) so the Application layer never references Identity directly.
public interface IIdentityGateway
{
    Task<OperationResult<RegisteredAccount>> RegisterAsync(
        string email, string password, string firstName, string lastName, string roleName, CancellationToken ct = default);

    Task<OperationResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct = default);

    Task<OperationResult<TokenResult>> LoginAsync(string email, string password, CancellationToken ct = default);

    // Data is null when the email doesn't resolve to an account — callers must still return a
    // generic success to their own caller, never revealing account existence.
    Task<OperationResult<PasswordResetToken?>> RequestPasswordResetAsync(string email, CancellationToken ct = default);

    Task<OperationResult> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken ct = default);
}
