using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Notifications;
using SGA.Web.Domain.Validation;

namespace SGA.Web.Infrastructure.Persistence.Validators;

public class AuthorizationQueryValidator : IAuthorizationQueryValidator
{
    private readonly WebAppDbContext _context;

    public AuthorizationQueryValidator(WebAppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationContext> ValidateOwnershipAsync(
        Guid requestingUserId, Guid authorizationId, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        var authorization = await _context.Authorizations.FirstOrDefaultAsync(a => a.Id == authorizationId, ct);

        if (authorization is null)
        {
            notifications.AddNotification("NotFound", "Authorization not found.");
            return notifications;
        }
        if (authorization.UserId != requestingUserId)
        {
            notifications.AddNotification("Forbidden", "You may not view another user's authorization.");
        }

        return notifications;
    }
}
