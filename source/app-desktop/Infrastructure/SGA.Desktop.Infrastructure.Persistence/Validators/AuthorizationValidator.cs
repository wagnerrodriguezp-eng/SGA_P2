using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Enums;
using SGA.SharedKernel.Domain.Notifications;
using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Domain.Validation;

namespace SGA.Desktop.Infrastructure.Persistence.Validators;

public class AuthorizationValidator : IAuthorizationValidator
{
    private readonly DesktopAppDbContext _context;

    public AuthorizationValidator(DesktopAppDbContext context)
    {
        _context = context;
    }

    public Task<NotificationContext> ValidateForCreateAsync(CreateAuthorizationDto dto, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();

        if (dto.AuthorizationType == AuthorizationType.MonthlyTicket)
        {
            if (!dto.EndDate.HasValue || dto.EndDate.Value <= dto.StartDate)
            {
                notifications.AddNotification("EndDate", "EndDate must be after StartDate for a monthly ticket.");
            }
        }
        else if (dto.AuthorizationType == AuthorizationType.RechargeableCard)
        {
            if (!dto.Balance.HasValue || dto.Balance.Value < 0)
            {
                notifications.AddNotification("Balance", "Balance must be zero or greater for a rechargeable card.");
            }
        }

        return Task.FromResult(notifications);
    }

    public async Task<NotificationContext> ValidateForUpdateAsync(Guid id, UpdateAuthorizationDto dto, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        var authorization = await _context.Authorizations.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (authorization is null)
        {
            notifications.AddNotification("NotFound", "Authorization not found.");
            return notifications;
        }

        // Cancellation is a one-way terminal state — matches the TripStatus state-machine discipline.
        if (authorization.AuthorizationStatus == AuthorizationStatus.Cancelled)
        {
            notifications.AddNotification("Cancelled", "A cancelled authorization cannot be modified.");
        }

        return notifications;
    }

    public async Task<NotificationContext> ValidateForDeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        var authorization = await _context.Authorizations.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (authorization is null)
        {
            notifications.AddNotification("NotFound", "Authorization not found.");
            return notifications;
        }
        if (authorization.AuthorizationStatus == AuthorizationStatus.Cancelled)
        {
            notifications.AddNotification("Cancelled", "This authorization is already cancelled.");
        }

        return notifications;
    }
}
