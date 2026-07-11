using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.Desktop.Application.Identity;

namespace SGA.Desktop.Api.Controllers;

public class LoginRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RefreshRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    private readonly IIdentityGateway _identityGateway;

    public AuthController(IIdentityGateway identityGateway)
    {
        _identityGateway = identityGateway;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var result = await _identityGateway.LoginAsync(dto.Email, dto.Password, ct);
        return FromResult(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto, CancellationToken ct)
    {
        var result = await _identityGateway.RefreshAsync(dto.RefreshToken, ct);
        return FromResult(result);
    }
}
