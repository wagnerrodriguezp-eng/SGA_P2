using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.Web.Application.Services;

namespace SGA.Web.Api.Controllers;

[Authorize(Policy = "StudentOrEmployee")]
public class AuthorizationsController : ApiControllerBase
{
    private readonly AuthorizationQueryService _authorizationQueryService;

    public AuthorizationsController(AuthorizationQueryService authorizationQueryService)
    {
        _authorizationQueryService = authorizationQueryService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var result = await _authorizationQueryService.GetMineAsync(CurrentUserId, ct);
        return FromResult(result);
    }
}
