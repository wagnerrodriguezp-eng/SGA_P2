using SGA.SharedKernel.Domain.Notifications;

namespace SGA.Web.Domain.Validation;

public interface ITripExecutionValidator
{
    Task<NotificationContext> ValidateStartAsync(Guid tripId, Guid driverUserId, CancellationToken ct = default);
    Task<NotificationContext> ValidateEndAsync(Guid tripId, Guid driverUserId, CancellationToken ct = default);
    Task<NotificationContext> ValidateIncidentReportAsync(Guid tripId, Guid reportedByUserId, CancellationToken ct = default);
    Task<NotificationContext> ValidateIncidentsQueryAsync(Guid tripId, Guid driverUserId, CancellationToken ct = default);
}
