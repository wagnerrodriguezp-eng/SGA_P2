using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.Desktop.Application.Services;
using SGA.Desktop.Domain.Dtos;

namespace SGA.Desktop.Api.Controllers;

public class ReportsController : ApiControllerBase
{
    private readonly ReportingService _reportingService;

    public ReportsController(ReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("usage")]
    public async Task<IActionResult> Usage([FromQuery] ReportFilterDto filter, CancellationToken ct) =>
        Ok(OperationResult<object>.Success(await _reportingService.GetUsageReportAsync(filter, ct)));

    [HttpGet("usage/records")]
    public async Task<IActionResult> UsageRecords([FromQuery] ReportFilterDto filter, CancellationToken ct) =>
        Ok(OperationResult<object>.Success(await _reportingService.GetUsageRecordsAsync(filter, ct)));

    [HttpGet("occupancy")]
    public async Task<IActionResult> Occupancy([FromQuery] ReportFilterDto filter, CancellationToken ct) =>
        Ok(OperationResult<object>.Success(await _reportingService.GetOccupancyReportAsync(filter, ct)));

    [HttpGet("punctuality")]
    public async Task<IActionResult> Punctuality([FromQuery] ReportFilterDto filter, CancellationToken ct) =>
        Ok(OperationResult<object>.Success(await _reportingService.GetPunctualityReportAsync(filter, ct)));

    [HttpGet("revenue")]
    public async Task<IActionResult> Revenue([FromQuery] ReportFilterDto filter, CancellationToken ct) =>
        Ok(OperationResult<object>.Success(await _reportingService.GetRevenueReportAsync(filter, ct)));
}
