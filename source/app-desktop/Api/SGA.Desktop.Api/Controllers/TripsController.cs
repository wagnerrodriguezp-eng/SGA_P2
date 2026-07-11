using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Services;
using SGA.Desktop.Domain.Dtos;

namespace SGA.Desktop.Api.Controllers;

public class TripsController : ApiControllerBase
{
    private readonly TripAssignmentService _tripAssignmentService;

    public TripsController(TripAssignmentService tripAssignmentService)
    {
        _tripAssignmentService = tripAssignmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
    {
        var trips = await _tripAssignmentService.GetAllForDateRangeAsync(from, to, ct);
        return Ok(OperationResult<IReadOnlyList<Trip>>.Success(trips));
    }

    [HttpPost]
    public async Task<IActionResult> Assign([FromBody] AssignTripDto dto, CancellationToken ct) =>
        FromResult(await _tripAssignmentService.AssignAsync(dto, ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelTripDto dto, CancellationToken ct)
    {
        dto.TripId = id;
        return FromResult(await _tripAssignmentService.CancelAsync(dto, ct));
    }
}
