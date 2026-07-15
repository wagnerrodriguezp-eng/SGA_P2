using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Desktop.Application.Services;

namespace SGA.Desktop.Api.Controllers;

public class IncidentsController : ApiControllerBase
{
    private readonly IncidentManagementService _incidentManagementService;

    public IncidentsController(IncidentManagementService incidentManagementService)
    {
        _incidentManagementService = incidentManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
    {
        var incidents = await _incidentManagementService.GetAllAsync(includeInactive, ct);
        return Ok(OperationResult<IReadOnlyList<Incident>>.Success(incidents));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var incident = await _incidentManagementService.GetByIdAsync(id, ct);
        return incident is null
            ? NotFound(OperationResult<Incident>.Failure(OperationResultStatus.NotFound, "Incident not found."))
            : Ok(OperationResult<Incident>.Success(incident));
    }

    [HttpGet("trip/{tripId:guid}")]
    public async Task<IActionResult> GetByTrip(Guid tripId, CancellationToken ct)
    {
        var incidents = await _incidentManagementService.GetByTripIdAsync(tripId, ct);
        return Ok(OperationResult<IReadOnlyList<Incident>>.Success(incidents));
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, CancellationToken ct) =>
        FromResult(await _incidentManagementService.UpdateStatusAsync(id, IncidentStatus.Resolved, ct));

    [HttpPost("{id:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken ct) =>
        FromResult(await _incidentManagementService.UpdateStatusAsync(id, IncidentStatus.Dismissed, ct));
}
