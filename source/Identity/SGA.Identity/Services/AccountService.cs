using Microsoft.AspNetCore.Identity;
using SGA.Identity.Entities;
using SGA.SharedKernel.Application.Results;

namespace SGA.Identity.Services;

public class AccountService : IAccountService
{
    private const string GenericLoginFailureMessage = "Invalid email or password.";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IApplicationAffinityChecker _affinityChecker;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        IApplicationAffinityChecker affinityChecker)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _affinityChecker = affinityChecker;
    }

    public async Task<OperationResult<RegisterAccountResult>> RegisterAsync(
        RegisterAccountRequest request, string roleName, CancellationToken ct = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return OperationResult<RegisterAccountResult>.Failure(
                OperationResultStatus.Conflict, "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return OperationResult<RegisterAccountResult>.Failure(
                OperationResultStatus.ValidationError,
                createResult.Errors.Select(e => e.Description).ToArray());
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new ApplicationRole(roleName));
        }
        await _userManager.AddToRoleAsync(user, roleName);

        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        return OperationResult<RegisterAccountResult>.Success(
            new RegisterAccountResult(user.Id, confirmationToken));
    }

    public async Task<OperationResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return OperationResult.Failure(OperationResultStatus.NotFound, "Account not found.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? OperationResult.Success("Email confirmed.")
            : OperationResult.Failure(OperationResultStatus.ValidationError, "Invalid or expired confirmation token.");
    }

    public async Task<OperationResult<AccessToken>> LoginAsync(
        string email, string password, IReadOnlyCollection<string> allowedRoleNames, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return OperationResult<AccessToken>.Failure(OperationResultStatus.Unauthorized, GenericLoginFailureMessage);
        }

        var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!passwordCheck.Succeeded)
        {
            // Covers wrong password, lockout, and RequireConfirmedEmail failures with one generic
            // message — never reveals which specific condition failed.
            return OperationResult<AccessToken>.Failure(OperationResultStatus.Unauthorized, GenericLoginFailureMessage);
        }

        var roles = (IReadOnlyCollection<string>)await _userManager.GetRolesAsync(user);
        if (!_affinityChecker.IsAllowed(roles))
        {
            // Application-affinity rule: a valid credential for a role that doesn't belong to this
            // app is rejected with the exact same generic message as a wrong password.
            return OperationResult<AccessToken>.Failure(OperationResultStatus.Unauthorized, GenericLoginFailureMessage);
        }

        var token = _tokenService.CreateAccessToken(user, roles);
        return OperationResult<AccessToken>.Success(token);
    }

    public async Task<OperationResult<PasswordResetRequestResult?>> RequestPasswordResetAsync(
        string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return OperationResult<PasswordResetRequestResult?>.Success(null);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return OperationResult<PasswordResetRequestResult?>.Success(new PasswordResetRequestResult(user.Id, token));
    }

    public async Task<OperationResult> ResetPasswordAsync(
        Guid userId, string token, string newPassword, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return OperationResult.Failure(OperationResultStatus.ValidationError, "Invalid or expired reset token.");
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded
            ? OperationResult.Success("Password reset successfully.")
            : OperationResult.Failure(OperationResultStatus.ValidationError, result.Errors.Select(e => e.Description).ToArray());
    }
}
