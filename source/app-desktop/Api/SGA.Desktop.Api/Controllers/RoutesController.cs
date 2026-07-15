using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.Desktop.Application.Services;
using SGA.Desktop.Domain.Dtos;
using Route = SGA.SharedKernel.Domain.Entities.Route;

namespace SGA.Desktop.Api.Controllers;

public class RoutesController : ApiControllerBase
{
    private readonly RouteSchedulingService _routeSchedulingService;

    public RoutesController(RouteSchedulingService routeSchedulingService)
    {
        _routeSchedulingService = routeSchedulingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
    {
        var routes = await _routeSchedulingService.GetAllRoutesAsync(includeInactive, ct);
        return Ok(OperationResult<IReadOnlyList<Route>>.Success(routes));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRouteDto dto, CancellationToken ct) =>
        FromResult(await _routeSchedulingService.CreateRouteAsync(dto, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRouteDto dto, CancellationToken ct) =>
        FromResult(await _routeSchedulingService.UpdateRouteAsync(id, dto, ct));

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct) =>
        FromResult(await _routeSchedulingService.DeactivateRouteAsync(id, ct));

    [HttpGet("{id:guid}/stops")]
    public async Task<IActionResult> GetStops(Guid id, CancellationToken ct)
    {
        var stops = await _routeSchedulingService.GetStopsForRouteAsync(id, ct);
        return Ok(OperationResult<IReadOnlyList<SGA.SharedKernel.Domain.Entities.Stop>>.Success(stops));
    }

    [HttpPost("{id:guid}/stops")]
    public async Task<IActionResult> CreateStop(Guid id, [FromBody] CreateStopDto dto, CancellationToken ct)
    {
        dto.RouteId = id;
        return FromResult(await _routeSchedulingService.CreateStopAsync(dto, ct));
    }

    [HttpPut("{id:guid}/stops/{stopId:guid}")]
    public async Task<IActionResult> UpdateStop(Guid id, Guid stopId, [FromBody] UpdateStopDto dto, CancellationToken ct) =>
        FromResult(await _routeSchedulingService.UpdateStopAsync(id, stopId, dto, ct));

    [HttpPost("{id:guid}/stops/{stopId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateStop(Guid id, Guid stopId, CancellationToken ct) =>
        FromResult(await _routeSchedulingService.DeactivateStopAsync(id, stopId, ct));

    [HttpGet("{id:guid}/schedules")]
    public async Task<IActionResult> GetSchedules(Guid id, CancellationToken ct)
    {
        var schedules = await _routeSchedulingService.GetSchedulesForRouteAsync(id, ct);
        return Ok(OperationResult<IReadOnlyList<SGA.SharedKernel.Domain.Entities.Schedule>>.Success(schedules));
    }

    [HttpPost("{id:guid}/schedules")]
    public async Task<IActionResult> CreateSchedule(Guid id, [FromBody] CreateScheduleDto dto, CancellationToken ct)
    {
        dto.RouteId = id;
        return FromResult(await _routeSchedulingService.CreateScheduleAsync(dto, ct));
    }
}
