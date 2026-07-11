using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Services;

namespace SGA.Desktop.Api.Controllers;

[Route("api/v{version:apiVersion}/audit-logs")]
public class AuditLogsController : ApiControllerBase
{
    private readonly AuditQueryService _auditQueryService;

    public AuditLogsController(AuditQueryService auditQueryService)
    {
        _auditQueryService = auditQueryService;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? userId, [FromQuery] string? action,
        CancellationToken ct)
    {
        var results = await _auditQueryService.SearchAsync(from, to, userId, action, ct);
        return Ok(OperationResult<IReadOnlyList<AuditLog>>.Success(results));
    }
}
