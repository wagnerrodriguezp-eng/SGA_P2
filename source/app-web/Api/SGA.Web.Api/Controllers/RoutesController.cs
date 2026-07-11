using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Web.Application.Services;
using Route = SGA.SharedKernel.Domain.Entities.Route;

namespace SGA.Web.Api.Controllers;

[Authorize]
public class RoutesController : ApiControllerBase
{
    private readonly ScheduleQueryService _scheduleQueryService;

    public RoutesController(ScheduleQueryService scheduleQueryService)
    {
        _scheduleQueryService = scheduleQueryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoutes(CancellationToken ct)
    {
        var routes = await _scheduleQueryService.GetRoutesAsync(ct);
        return Ok(OperationResult<IReadOnlyList<Route>>.Success(routes));
    }

    [HttpGet("{id:guid}/stops")]
    public async Task<IActionResult> GetStops(Guid id, CancellationToken ct)
    {
        var stops = await _scheduleQueryService.GetStopsForRouteAsync(id, ct);
        return Ok(OperationResult<IReadOnlyList<Stop>>.Success(stops));
    }
}
