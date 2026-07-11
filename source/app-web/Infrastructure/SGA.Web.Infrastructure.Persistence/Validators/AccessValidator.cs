using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Enums;
using SGA.SharedKernel.Domain.Notifications;
using SGA.Web.Domain.Validation;

namespace SGA.Web.Infrastructure.Persistence.Validators;

// Implements the three-condition access check from
// shared-kernel/05-business-rules-to-domain-mapping.md §1 steps 1-4. Notification keys
// ("Unauthorized"/"Expired"/"NoBalance"/"NoCapacity") are read back by AccessValidationService to
// map the denial onto the corresponding AccessResult value.
public class AccessValidator : IAccessValidator
{
    private readonly WebAppDbContext _context;

    public AccessValidator(WebAppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationContext> ValidateAccessAsync(Guid userId, Guid tripId, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();

        var authorization = await _context.Authorizations
            .Where(a => a.UserId == userId && a.AuthorizationStatus == AuthorizationStatus.Active)
            .FirstOrDefaultAsync(ct);

        if (authorization is null)
        {
            notifications.AddNotification("Unauthorized", "No active authorization found for this user.");
            return notifications;
        }

        if (authorization.AuthorizationType == AuthorizationType.MonthlyTicket)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var withinRange = today >= authorization.StartDate &&
                               (!authorization.EndDate.HasValue || today <= authorization.EndDate.Value);
            if (!withinRange)
            {
                notifications.AddNotification("Expired", "Your monthly ticket is not valid today.");
                return notifications;
            }
        }
        else if (authorization.AuthorizationType == AuthorizationType.RechargeableCard)
        {
            if (!authorization.Balance.HasValue || authorization.Balance.Value <= 0)
            {
                notifications.AddNotification("NoBalance", "Your card has no remaining trips.");
                return notifications;
            }
        }

        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct);
        if (trip is null || trip.CapacityUsed >= trip.MaxCapacitySnapshot)
        {
            notifications.AddNotification("NoCapacity", "This trip has no remaining capacity.");
        }

        return notifications;
    }
}
