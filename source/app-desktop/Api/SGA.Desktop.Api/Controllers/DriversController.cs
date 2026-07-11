using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Services;
using SGA.Desktop.Domain.Dtos;

namespace SGA.Desktop.Api.Controllers;

public class DriversController : ApiControllerBase
{
    private readonly DriverManagementService _driverManagementService;
    private readonly IConfiguration _configuration;

    public DriversController(DriverManagementService driverManagementService, IConfiguration configuration)
    {
        _driverManagementService = driverManagementService;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
    {
        var drivers = await _driverManagementService.GetAllAsync(includeInactive, ct);
        return Ok(OperationResult<IReadOnlyList<DriverProfile>>.Success(drivers));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDriverDto dto, CancellationToken ct)
    {
        var confirmationLinkBase = $"{_configuration["WebClientApp:BaseUrl"]}/Account/ConfirmEmail";
        return FromResult(await _driverManagementService.CreateAsync(dto, confirmationLinkBase, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDriverDto dto, CancellationToken ct) =>
        FromResult(await _driverManagementService.UpdateAsync(id, dto, ct));

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct) =>
        FromResult(await _driverManagementService.DeactivateAsync(id, ct));
}
