using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Web.Application.Dtos;
using SGA.Web.Application.Services;

namespace SGA.Web.Api.Controllers;

[Authorize(Policy = "DriverOnly")]
public class TripsController : ApiControllerBase
{
    private readonly TripExecutionService _tripExecutionService;

    public TripsController(TripExecutionService tripExecutionService)
    {
        _tripExecutionService = tripExecutionService;
    }

    [HttpGet("assigned")]
    public async Task<IActionResult> GetAssigned(CancellationToken ct)
    {
        var trips = await _tripExecutionService.GetAssignedTodayAsync(CurrentUserId, ct);
        return Ok(OperationResult<IReadOnlyList<Trip>>.Success(trips));
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        var result = await _tripExecutionService.StartTripAsync(id, CurrentUserId, ct);
        return FromResult(result);
    }

    [HttpPost("{id:guid}/end")]
    public async Task<IActionResult> End(Guid id, CancellationToken ct)
    {
        var result = await _tripExecutionService.EndTripAsync(id, CurrentUserId, ct);
        return FromResult(result);
    }

    [HttpPost("{id:guid}/incidents")]
    public async Task<IActionResult> ReportIncident(Guid id, [FromBody] ReportIncidentRequestDto dto, CancellationToken ct)
    {
        var result = await _tripExecutionService.ReportIncidentAsync(id, CurrentUserId, dto, ct);
        return FromResult(result);
    }
}
