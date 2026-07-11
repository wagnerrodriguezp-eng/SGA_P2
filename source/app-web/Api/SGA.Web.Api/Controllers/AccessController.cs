using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.Web.Application.Dtos;
using SGA.Web.Application.Services;

namespace SGA.Web.Api.Controllers;

[Authorize(Policy = "StudentOrEmployee")]
public class AccessController : ApiControllerBase
{
    private readonly AccessValidationService _accessValidationService;

    public AccessController(AccessValidationService accessValidationService)
    {
        _accessValidationService = accessValidationService;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateAccessRequestDto dto, CancellationToken ct)
    {
        var result = await _accessValidationService.ValidateAndRecordAsync(CurrentUserId, dto.TripId, ct);
        return FromResult(result);
    }
}
