using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Services;
using SGA.Desktop.Domain.Dtos;

namespace SGA.Desktop.Api.Controllers;

public class AuthorizationsController : ApiControllerBase
{
    private readonly AuthorizationManagementService _authorizationManagementService;

    public AuthorizationsController(AuthorizationManagementService authorizationManagementService)
    {
        _authorizationManagementService = authorizationManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetForUser([FromQuery] Guid userId, CancellationToken ct)
    {
        var authorizations = await _authorizationManagementService.GetForUserAsync(userId, ct);
        return Ok(OperationResult<IReadOnlyList<Authorization>>.Success(authorizations));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuthorizationDto dto, CancellationToken ct) =>
        FromResult(await _authorizationManagementService.CreateAsync(dto, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAuthorizationDto dto, CancellationToken ct) =>
        FromResult(await _authorizationManagementService.UpdateAsync(id, dto, ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct) =>
        FromResult(await _authorizationManagementService.CancelAsync(id, ct));
}
