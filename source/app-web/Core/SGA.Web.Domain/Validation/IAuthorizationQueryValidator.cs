using SGA.SharedKernel.Domain.Notifications;

namespace SGA.Web.Domain.Validation;

public interface IAuthorizationQueryValidator
{
    Task<NotificationContext> ValidateOwnershipAsync(Guid requestingUserId, Guid authorizationId, CancellationToken ct = default);
}
