using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Web.Application.Services;

namespace SGA.Web.Api.Controllers;

[Authorize]
public class SchedulesController : ApiControllerBase
{
    private readonly ScheduleQueryService _scheduleQueryService;

    public SchedulesController(ScheduleQueryService scheduleQueryService)
    {
        _scheduleQueryService = scheduleQueryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedules([FromQuery] Guid? routeId, CancellationToken ct)
    {
        var schedules = await _scheduleQueryService.GetSchedulesAsync(routeId, ct);
        return Ok(OperationResult<IReadOnlyList<Schedule>>.Success(schedules));
    }
}
