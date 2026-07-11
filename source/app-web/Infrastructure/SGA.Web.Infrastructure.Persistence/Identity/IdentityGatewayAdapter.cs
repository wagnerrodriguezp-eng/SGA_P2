using SGA.Identity.Constants;
using SGA.Identity.Services;
using SGA.SharedKernel.Application.Results;
using SGA.Web.Application.Identity;

namespace SGA.Web.Infrastructure.Persistence.Identity;

// Adapts SGA.Identity's IAccountService to the Application-facing IIdentityGateway abstraction so
// SGA.Web.Application never references SGA.Identity directly.
public class IdentityGatewayAdapter : IIdentityGateway
{
    private static readonly string[] AllowedLoginRoles = { RoleNames.Student, RoleNames.Employee, RoleNames.Driver };

    private readonly IAccountService _accountService;

    public IdentityGatewayAdapter(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<OperationResult<RegisteredAccount>> RegisterAsync(
        string email, string password, string firstName, string lastName, string roleName, CancellationToken ct = default)
    {
        var result = await _accountService.RegisterAsync(
            new RegisterAccountRequest(email, password, firstName, lastName), roleName, ct);

        if (!result.IsSuccess || result.Data is null)
        {
            return OperationResult<RegisteredAccount>.Failure(result.Status, result.Errors.ToArray());
        }

        return OperationResult<RegisteredAccount>.Success(
            new RegisteredAccount(result.Data.UserId, result.Data.EmailConfirmationToken));
    }

    public Task<OperationResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct = default) =>
        _accountService.ConfirmEmailAsync(userId, token, ct);

    public async Task<OperationResult<TokenResult>> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var result = await _accountService.LoginAsync(email, password, AllowedLoginRoles, ct);
        if (!result.IsSuccess || result.Data is null)
        {
            return OperationResult<TokenResult>.Failure(result.Status, result.Errors.ToArray());
        }

        return OperationResult<TokenResult>.Success(new TokenResult(result.Data.Token, result.Data.ExpiresAtUtc));
    }

    public async Task<OperationResult<PasswordResetToken?>> RequestPasswordResetAsync(string email, CancellationToken ct = default)
    {
        var result = await _accountService.RequestPasswordResetAsync(email, ct);
        if (!result.IsSuccess)
        {
            return OperationResult<PasswordResetToken?>.Failure(result.Status, result.Errors.ToArray());
        }

        var data = result.Data is null ? null : new PasswordResetToken(result.Data.UserId, result.Data.ResetToken);
        return OperationResult<PasswordResetToken?>.Success(data);
    }

    public Task<OperationResult> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken ct = default) =>
        _accountService.ResetPasswordAsync(userId, token, newPassword, ct);
}
