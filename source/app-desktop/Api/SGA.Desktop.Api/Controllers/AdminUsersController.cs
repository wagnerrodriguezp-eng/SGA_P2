using Microsoft.AspNetCore.Mvc;
using SGA.Desktop.Application.Services;
using SGA.Desktop.Domain.Dtos;

namespace SGA.Desktop.Api.Controllers;

[Route("api/v{version:apiVersion}/admin/users")]
public class AdminUsersController : ApiControllerBase
{
    private readonly UserManagementService _userManagementService;
    private readonly IConfiguration _configuration;

    public AdminUsersController(UserManagementService userManagementService, IConfiguration configuration)
    {
        _userManagementService = userManagementService;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Provision([FromBody] RegisterAccountRequestDto dto, CancellationToken ct)
    {
        var confirmationLinkBase = $"{_configuration["WebClientApp:BaseUrl"]}/Account/ConfirmEmail";
        return FromResult(await _userManagementService.ProvisionAccountAsync(dto, confirmationLinkBase, ct));
    }
}
