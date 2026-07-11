using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGA.Identity.Constants;
using SGA.Identity.Entities;
using SGA.Identity.Services;
using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Identity;

namespace SGA.Desktop.Infrastructure.Persistence.Identity;

// Adapts SGA.Identity's account/token services plus this app's own RefreshToken store to the
// Application-facing IIdentityGateway abstraction. The refresh token is persisted only here, in
// this app's own database — SGA.Identity itself is never taught about refresh tokens.
public class IdentityGatewayAdapter : IIdentityGateway
{
    private static readonly string[] AllowedLoginRoles = { RoleNames.TransportAdmin };
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IAccountService _accountService;
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGenericRepository<RefreshToken, Guid> _refreshTokens;
    private readonly DesktopAppDbContext _context;

    public IdentityGatewayAdapter(
        IAccountService accountService,
        ITokenService tokenService,
        UserManager<ApplicationUser> userManager,
        IGenericRepository<RefreshToken, Guid> refreshTokens,
        DesktopAppDbContext context)
    {
        _accountService = accountService;
        _tokenService = tokenService;
        _userManager = userManager;
        _refreshTokens = refreshTokens;
        _context = context;
    }

    public async Task<OperationResult<TokenPairResult>> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var result = await _accountService.LoginAsync(email, password, AllowedLoginRoles, ct);
        if (!result.IsSuccess || result.Data is null)
        {
            return OperationResult<TokenPairResult>.Failure(result.Status, result.Errors.ToArray());
        }

        var pair = await IssueTokenPairAsync(result.Data.Token, result.Data.ExpiresAtUtc, result.Data.UserId, ct);
        return OperationResult<TokenPairResult>.Success(pair);
    }

    public async Task<OperationResult<TokenPairResult>> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var existing = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken, ct);
        if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc < DateTime.UtcNow)
        {
            return OperationResult<TokenPairResult>.Failure(OperationResultStatus.Unauthorized, "Invalid or expired refresh token.");
        }

        var user = await _userManager.FindByIdAsync(existing.UserId.ToString());
        if (user is null)
        {
            return OperationResult<TokenPairResult>.Failure(OperationResultStatus.Unauthorized, "Invalid or expired refresh token.");
        }

        var roles = (IReadOnlyCollection<string>)await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.CreateAccessToken(user, roles);

        var newRefreshValue = GenerateSecureToken();
        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedByToken = newRefreshValue;
        await _refreshTokens.UpdateAsync(existing, ct);

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = existing.UserId,
            Token = newRefreshValue,
            ExpiresAtUtc = DateTime.UtcNow.Add(RefreshTokenLifetime)
        };
        await _refreshTokens.AddAsync(newRefreshToken, ct);
        await _refreshTokens.SaveChangesAsync(ct);

        return OperationResult<TokenPairResult>.Success(
            new TokenPairResult(newAccessToken.Token, newAccessToken.ExpiresAtUtc, newRefreshValue));
    }

    public async Task<OperationResult<RegisteredAccount>> RegisterAccountAsync(
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

    public async Task<IReadOnlyDictionary<Guid, string>> GetUserEmailsAsync(
        IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var users = await _context.Users.Where(u => ids.Contains(u.Id)).ToListAsync(ct);
        return users
            .Where(u => !string.IsNullOrEmpty(u.Email))
            .ToDictionary(u => u.Id, u => u.Email!);
    }

    private async Task<TokenPairResult> IssueTokenPairAsync(
        string accessToken, DateTime accessTokenExpiresAtUtc, Guid userId, CancellationToken ct)
    {
        var refreshValue = GenerateSecureToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = refreshValue,
            ExpiresAtUtc = DateTime.UtcNow.Add(RefreshTokenLifetime)
        };
        await _refreshTokens.AddAsync(refreshToken, ct);
        await _refreshTokens.SaveChangesAsync(ct);

        return new TokenPairResult(accessToken, accessTokenExpiresAtUtc, refreshValue);
    }

    private static string GenerateSecureToken() =>
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
}
