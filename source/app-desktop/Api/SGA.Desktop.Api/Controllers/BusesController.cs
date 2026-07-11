using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Services;
using SGA.Desktop.Domain.Dtos;

namespace SGA.Desktop.Api.Controllers;

public class BusesController : ApiControllerBase
{
    private readonly FleetManagementService _fleetManagementService;

    public BusesController(FleetManagementService fleetManagementService)
    {
        _fleetManagementService = fleetManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
    {
        var buses = await _fleetManagementService.GetAllAsync(includeInactive, ct);
        return Ok(OperationResult<IReadOnlyList<Bus>>.Success(buses));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var bus = await _fleetManagementService.GetByIdAsync(id, ct);
        return bus is null
            ? NotFound(OperationResult<Bus>.Failure(OperationResultStatus.NotFound, "Bus not found."))
            : Ok(OperationResult<Bus>.Success(bus));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusDto dto, CancellationToken ct) =>
        FromResult(await _fleetManagementService.CreateAsync(dto, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBusDto dto, CancellationToken ct) =>
        FromResult(await _fleetManagementService.UpdateAsync(id, dto, ct));

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct) =>
        FromResult(await _fleetManagementService.DeactivateAsync(id, ct));
}
