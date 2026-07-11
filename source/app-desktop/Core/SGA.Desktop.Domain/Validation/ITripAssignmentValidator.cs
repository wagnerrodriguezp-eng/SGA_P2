using SGA.SharedKernel.Domain.Notifications;

namespace SGA.Desktop.Domain.Validation;

public interface ITripAssignmentValidator
{
    Task<NotificationContext> ValidateAssignmentAsync(
        Guid scheduleId, Guid busId, Guid driverUserId, DateOnly tripDate, CancellationToken ct = default);
}
