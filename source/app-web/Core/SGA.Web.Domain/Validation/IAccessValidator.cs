using SGA.SharedKernel.Domain.Notifications;

namespace SGA.Web.Domain.Validation;

public interface IAccessValidator
{
    Task<NotificationContext> ValidateAccessAsync(Guid userId, Guid tripId, CancellationToken ct = default);
}
