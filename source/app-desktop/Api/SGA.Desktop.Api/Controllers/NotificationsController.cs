using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Services;

namespace SGA.Desktop.Api.Controllers;

// Read-only: NotificationMessage rows are system-managed (see NotificationMonitoringService).
public class NotificationsController : ApiControllerBase
{
    private readonly NotificationMonitoringService _notificationMonitoringService;

    public NotificationsController(NotificationMonitoringService notificationMonitoringService)
    {
        _notificationMonitoringService = notificationMonitoringService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive, CancellationToken ct)
    {
        var messages = await _notificationMonitoringService.GetAllAsync(includeInactive, ct);
        return Ok(OperationResult<IReadOnlyList<NotificationMessage>>.Success(messages));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var message = await _notificationMonitoringService.GetByIdAsync(id, ct);
        return message is null
            ? NotFound(OperationResult<NotificationMessage>.Failure(OperationResultStatus.NotFound, "Notification not found."))
            : Ok(OperationResult<NotificationMessage>.Success(message));
    }
}
