using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.Web.Application.Dtos;
using SGA.Web.Application.Services;

namespace SGA.Web.Api.Controllers;

[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    private readonly AccountService _accountService;
    private readonly IConfiguration _configuration;

    public AuthController(AccountService accountService, IConfiguration configuration)
    {
        _accountService = accountService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAccountRequestDto dto, CancellationToken ct)
    {
        var confirmationLinkBase = $"{ClientAppBaseUrl}/Account/ConfirmEmail";
        var result = await _accountService.RegisterAsync(dto, confirmationLinkBase, ct);
        return FromResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var result = await _accountService.LoginAsync(dto.Email, dto.Password, ct);
        return FromResult(result);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestDto dto, CancellationToken ct)
    {
        var result = await _accountService.ConfirmEmailAsync(dto.UserId, dto.Token, ct);
        return FromResult(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto, CancellationToken ct)
    {
        var resetLinkBase = $"{ClientAppBaseUrl}/Account/ResetPassword";
        var result = await _accountService.RequestPasswordResetAsync(dto.Email, resetLinkBase, ct);
        return FromResult(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken ct)
    {
        var result = await _accountService.ResetPasswordAsync(dto.UserId, dto.Token, dto.NewPassword, ct);
        return FromResult(result);
    }

    private string ClientAppBaseUrl => _configuration["ClientApp:BaseUrl"] ?? string.Empty;
}
