using SGA.SharedKernel.Application.Results;

namespace SGA.Identity.Services;

public record RegisterAccountRequest(string Email, string Password, string FirstName, string LastName);

public record RegisterAccountResult(Guid UserId, string EmailConfirmationToken);

public record PasswordResetRequestResult(Guid UserId, string ResetToken);

public interface IAccountService
{
    Task<OperationResult<RegisterAccountResult>> RegisterAsync(
        RegisterAccountRequest request, string roleName, CancellationToken ct = default);

    Task<OperationResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct = default);

    // allowedRoleNames enforces the application-affinity rule: a valid credential for a role not
    // in this set is rejected with the exact same generic failure as a wrong password.
    Task<OperationResult<AccessToken>> LoginAsync(
        string email, string password, IReadOnlyCollection<string> allowedRoleNames, CancellationToken ct = default);

    // Data is null when the email doesn't resolve to an account — callers must still return a
    // generic success message to the caller in that case, never revealing account existence.
    Task<OperationResult<PasswordResetRequestResult?>> RequestPasswordResetAsync(
        string email, CancellationToken ct = default);

    Task<OperationResult> ResetPasswordAsync(
        Guid userId, string token, string newPassword, CancellationToken ct = default);
}
